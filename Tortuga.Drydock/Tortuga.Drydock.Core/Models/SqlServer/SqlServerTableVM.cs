using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tortuga.Chain;
using Tortuga.Chain.SqlServer;

namespace Tortuga.Drydock.Models.SqlServer
{
    public class SqlServerTableVM : TableVM<SqlServerObjectName, SqlDbType>
    {
        static readonly Regex DomainRegex = new Regex(@"^\w+[\\]\w+$", RegexOptions.Compiled);
        static readonly Regex FileRegex = new Regex("(^(PRN|AUX|NUL|CON|COM[1-9]|LPT[1-9]|(\\.+)$)(\\..*)?$)|(([\\x00-\\x1f\\\\?*:\"​|/<>‌​])+)|(^([\\.]+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public SqlServerTableVM(SqlServerDataSource dataSource, SqlServerTableOrViewMetadata<SqlDbType> table) : base(dataSource, table)
        {
            FixItOperations.Add(new FixNulls(this));
            FixItOperations.Add(new FixSparse(this));
            FixItOperations.Add(new FixAddIdentityColumn(this));
            FixItOperations.Add(new FixObsoleteTypes(this));
        }

        public override string QuotedTableName => Table.Name.ToQuotedString();

        public new SqlServerDataSource DataSource { get => (SqlServerDataSource)base.DataSource; }

        public long ObjectId { get => Get<long>(); set => Set(value); }

        protected override void PopulateColumns()
        {
            Columns.AddRange(Table.Columns.Select((c, i) => new SqlServerColumnModel(c) { SortIndex = i }));
            foreach (var column in Columns)
            {
                column.FixItOperations.Add(new FixEmailCheckConstraint(this, column));
                column.FixItOperations.Add(new FixDomainUserNameCheckConstraint(this, column));
                column.FixItOperations.Add(new FixNotEmptyCheckConstraint(this, column));
                column.FixItOperations.Add(new FixBooleanAsText(this, column));
                column.FixItOperations.Add(new FixIntegerAsText(this, column));
                column.FixItOperations.Add(new FixDecimalAsText(this, column));
                column.FixItOperations.Add(new FixUnusedColumn(this, column));
            }
        }

        public override bool SupportsSparse { get => true; }


        protected override Task AnalyzeColumnAsync(ColumnModel<SqlDbType> column) => AnalyzeColumnAsync((SqlServerColumnModel)column);

        protected async Task AnalyzeColumnAsync(SqlServerColumnModel column)
        {
            if (column.StatsLoaded)
                return;

            StartWork();
            try
            {

                var sql = new StringBuilder(@"SELECT COUNT(*) AS SampleSize");
                if (column.SupportsDistinct)
                    sql.Append($@", COUNT(DISTINCT {column.Column.QuotedSqlName}) AS DistinctCount");
                if (column.IsNullable)
                    sql.Append($@", SUM( CASE WHEN {column.Column.QuotedSqlName} IS NULL THEN 1 ELSE 0 END) AS [NullCount]");
                if (column.ContainsText)
                {
                    sql.Append($@", SUM( CASE WHEN {column.Column.QuotedSqlName} = '' THEN 1 ELSE 0 END) AS [EmptyCount]");
                    sql.Append($@", AVG(LEN( NULLIF({column.Column.QuotedSqlName}, '')) * 1.0) AS AverageLength");
                    sql.Append($@", CONVERT( INT,  MAX( LEN ( {column.Column.QuotedSqlName} ) )) AS [ActualMaxLength]");
                }
                sql.Append($" FROM {Table.Name.ToQuotedString()} TABLESAMPLE ({MaxSampleSize} ROWS) WITH (READUNCOMMITTED)");

                var row = await DataSource.Sql(sql.ToString(), null).ToRow().ExecuteAsync();

                column.SampleSize = (int)row["SampleSize"];
                if (column.SampleSize > 0)
                {
                    if (column.SupportsDistinct)
                    {
                        column.DistinctCount = (int)row["DistinctCount"];
                        if (column.IsUnique == null)
                            column.IsUnique = column.DistinctCount == column.SampleSize;
                    }
                    if (column.IsNullable)
                        column.NullCount = (int)row["NullCount"];
                    if (column.ContainsText)
                    {
                        column.EmptyCount = (int)row["EmptyCount"];
                        column.AverageLength = (decimal?)row["AverageLength"];
                        column.ActualMaxLength = (int?)row["ActualMaxLength"];
                    }
                }


                const string constraintSql = @"SELECT 
cc.name AS ConstraintName,
cc.definition AS [Definition],
2 AS ConstraintType
 FROM Sys.check_constraints cc
INNER JOIN sys.columns c ON cc.parent_object_id = c.object_id AND cc.parent_column_id = c.column_id
INNER JOIN sys.tables t ON c.object_id = t.object_id
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name =@Schema AND t.name=@Name AND c.name = @Column;";

                column.Constraints.AddRange(await DataSource.Sql(constraintSql, new { Table.Name.Name, Table.Name.Schema, Column = column.Column.SqlName }).ToCollection<Constraint>().ExecuteAsync());


                const string fkSql = @"SELECT s2.name + '.' + t2.name + '(' + c2.name + ')' AS Definition,
       fk.name AS ConstraintName,
       1 AS ConstraintType,
       s2.name AS ReferencedSchemaName,
       t2.name AS ReferencedTableName,
       c2.name AS ReferencedColumnName
FROM sys.foreign_key_columns fkc
    INNER JOIN sys.tables t
        ON fkc.parent_object_id = t.object_id
    INNER JOIN sys.columns c
        ON c.column_id = fkc.parent_column_id
           AND c.object_id = fkc.parent_object_id
    INNER JOIN sys.schemas s
        ON s.schema_id = t.schema_id
    INNER JOIN sys.tables t2
        ON fkc.referenced_object_id = t2.object_id
    INNER JOIN sys.columns c2
        ON c2.column_id = fkc.referenced_column_id
           AND c2.object_id = fkc.referenced_object_id
    INNER JOIN sys.schemas s2
        ON s2.schema_id = t2.schema_id
    INNER JOIN sys.foreign_keys fk
        ON fk.object_id = fkc.constraint_object_id
WHERE s.name =@Schema AND t.name=@Name AND c.name = @Column;";

                column.Constraints.AddRange(await DataSource.Sql(fkSql, new { Table.Name.Name, Table.Name.Schema, Column = column.Column.SqlName }).ToCollection<Constraint>().ExecuteAsync());

                const string dcSql = @"SELECT dc.name AS ConstraintName,
       0 AS ConstraintType,
       dc.definition AS Definition
FROM sys.default_constraints dc
    INNER JOIN sys.columns c
        ON c.column_id = dc.parent_column_id
           AND c.object_id = dc.parent_object_id
    INNER JOIN sys.tables t
        ON dc.parent_object_id = t.object_id
    INNER JOIN sys.schemas s
        ON s.schema_id = t.schema_id
WHERE s.name =@Schema AND t.name=@Name AND c.name = @Column;";

                column.Constraints.AddRange(await DataSource.Sql(dcSql, new { Table.Name.Name, Table.Name.Schema, Column = column.Column.SqlName }).ToCollection<Constraint>().ExecuteAsync());

                column.ConstraintsLoaded = true;

                if (column.ContainsText && column.ActualMaxLength < 300)
                {
                    var topHundred = await OnShowTopTenAsync(column, 100);
                    if (topHundred.Rows.Count > 0)
                    {


                        bool isEmail = true;
                        bool isDateTime = true;
                        bool isDomainUserName = true;
                        bool isFile = true;
                        bool isInteger = true;
                        bool isBoolean = true;
                        bool isDecimal = true;
                        bool nonNullFound = false;
                        long maxLong = long.MinValue; //TODO: Fixit will determine the best data type

                        var i = 0;

                        while (i < topHundred.Rows.Count && (isEmail || isDateTime || isDomainUserName || isFile))
                        {
                            var value = topHundred.Rows[i]["Value"] as String;
                            if (value != null)
                            {
                                nonNullFound = true;
                                isEmail = isEmail && value.Contains('@') && MimeKit.MailboxAddress.TryParse(value, out var _);
                                isDateTime = isDateTime && DateTime.TryParse(value, out var _);
                                isDomainUserName = isDomainUserName && DomainRegex.IsMatch(value);
                                isFile = isFile && IsFileName(value);

                                if (isInteger && long.TryParse(value, out var number))
                                {
                                    maxLong = Math.Max(maxLong, number);
                                }
                                else
                                {
                                    isInteger = false;
                                }

                                isDecimal = isDecimal && float.TryParse(value, out var _);

                                isBoolean = isBoolean && IsBoolean(value);

                            }
                            i += 1;

                        }

                        if (nonNullFound)
                        {
                            column.TextContentFeatures = TextContentFeatures.None;
                            if (isEmail)
                                column.TextContentFeatures = column.TextContentFeatures | TextContentFeatures.EmailAddress;
                            if (isDateTime)
                                column.TextContentFeatures = column.TextContentFeatures | TextContentFeatures.DateTime;
                            if (isDomainUserName)
                                column.TextContentFeatures = column.TextContentFeatures | TextContentFeatures.DomainUserName;
                            if (isFile && !isEmail)
                                column.TextContentFeatures = column.TextContentFeatures | TextContentFeatures.FileName;
                            if (isInteger)
                                column.TextContentFeatures = column.TextContentFeatures | TextContentFeatures.Integer;
                            else if (isDecimal)
                                column.TextContentFeatures = column.TextContentFeatures | TextContentFeatures.Decimal;
                            if (isBoolean)
                                column.TextContentFeatures = column.TextContentFeatures | TextContentFeatures.Boolean;
                        }
                    }
                }

                column.FixItOperations.RefreshAll();

                column.StatsLoaded = true;

            }

            catch (Exception ex)
            {
                Status = ex.ToString();
                throw;
            }
            finally
            {
                StopWork();

            }
        }

        private static bool IsBoolean(string value)
        {
            switch (value.ToUpperInvariant())
            {
                case "1":
                case "0":
                case "TRUE":
                case "FALSE":
                case "T":
                case "F":
                case "Y":
                case "YES":
                case "N":
                case "NO":
                    return true;
                default:
                    return false;
            }
        }

        static bool IsFileName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                if (!Path.HasExtension(path))
                    return false; //not a file name. might be a full path

                //if (Path.GetFileName(path) != path)
                //    return false; //this is full path name, not a file name
            }
            catch
            {
                return false;
            }

            return true;

        }

        public bool ShowAddIdentityColumn => IsHeap == true;


        protected override Task<DataTable> OnShowTopTenAsync(ColumnModel<SqlDbType> column, int rowCount)
        {
            return DataSource.Sql($"SELECT TOP {rowCount} {column.Column.QuotedSqlName} AS Value, COUNT(*) AS Count FROM {Table.Name.ToQuotedString()} TABLESAMPLE ({MaxSampleSize} ROWS) WITH (READUNCOMMITTED) GROUP BY {column.Column.QuotedSqlName} ORDER BY COUNT(*) DESC").ToDataTable().ExecuteAsync();
        }


    }
}


