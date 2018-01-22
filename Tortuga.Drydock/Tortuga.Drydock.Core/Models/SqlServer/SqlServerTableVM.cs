using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tortuga.Chain;
using Tortuga.Chain.SqlServer;

namespace Tortuga.Drydock.Models.SqlServer
{
    public class SqlServerTableVM : TableVM<SqlServerObjectName, SqlDbType>
    {
        public SqlServerTableVM(SqlServerDataSource dataSource, SqlServerTableOrViewMetadata<SqlDbType> table) : base(dataSource, table)
        {
            FixItOperations.Add(new FixNulls(this));
            FixItOperations.Add(new FixSparse(this));
            FixItOperations.Add(new FixAddIdentityColumn(this));
            FixItOperations.Add(new FixObsoleteTypes(this));
        }

        public new SqlServerDataSource DataSource { get => (SqlServerDataSource)base.DataSource; }

        public long ObjectId { get => Get<long>(); set => Set(value); }

        protected override void PopulateColumns()
        {
            Columns.AddRange(Table.Columns.Select(c => new SqlServerColumnModel(c)));
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

        public bool ShowAddIdentityColumn => IsHeap == true;


        protected override Task<DataTable> OnShowTopTenAsync(ColumnModel<SqlDbType> column)
        {
            return DataSource.Sql($"SELECT TOP 10 ISNULL({column.Column.QuotedSqlName}, '<NULL>') AS Value, COUNT(*) AS Count FROM {Table.Name.ToQuotedString()} TABLESAMPLE ({MaxSampleSize} ROWS) WITH (READUNCOMMITTED) GROUP BY {column.Column.QuotedSqlName} ORDER BY COUNT(*) DESC").ToDataTable().ExecuteAsync();
        }


    }
}


