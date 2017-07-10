using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Tortuga.Chain;
using Tortuga.Chain.SqlServer;

namespace Tortuga.Drydock.Models.SqlServer
{
    public class SqlServerTableVM : TableVM<SqlServerObjectName, SqlDbType>
    {
        public SqlServerTableVM(SqlServerDataSource dataSource, SqlServerTableOrViewMetadata<SqlDbType> table) : base(dataSource, table)
        {
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

                if (column.SparseCandidate || column.SparseWarning)
                    ShowSparseFixIt = true;
                if (column.NullCount == 0)
                    ShowNullFixIt = true;
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

        public bool ShowSparseFixIt { get => Get<bool>(); private set => Set(value); }


        public ICommand FixSparseCommand => GetCommand(FixSparse);

        void FixSparse()
        {
            var change = new StringBuilder();
            change.AppendLine($"USE [{DataSource.Name}]"); //Task-25 replace this with something more reliable.
            foreach (var column in Columns.Cast<SqlServerColumnModel>().Where(c => c.SparseCandidate))
            {
                change.AppendLine($"ALTER TABLE {Table.Name.ToQuotedString()} ALTER COLUMN [{column.Name}] {column.Column.FullTypeName} SPARSE");
            }
            foreach (var column in Columns.Cast<SqlServerColumnModel>().Where(c => c.SparseWarning))
            {
                change.AppendLine($"ALTER TABLE {Table.Name.ToQuotedString()} ALTER COLUMN [{column.Name}] {column.Column.FullTypeName} NULL");
            }

            var model = new FixItVM()
            {
                WindowTitle = $"Sparse columns for {Table.Name.ToString()}",
                ChangeSql = change.ToString()
            };
            RequestDialog(model);
        }

        public override bool SupportsFixNull => true;
        public override bool SupportsAnalyzeColumn => true;

        protected override void FixNull()
        {
            var change = new StringBuilder();
            change.AppendLine($"USE [{DataSource.Name}]");
            foreach (var column in Columns.Where(c => c.IsNullable && c.NullCount == 0))
            {
                change.AppendLine($"ALTER TABLE {Table.Name.ToQuotedString()} ALTER COLUMN [{column.Name}] {column.Column.FullTypeName} NOT NULL");
            }

            var model = new FixItVM()
            {
                WindowTitle = $"Nullable columns without nulls for {Table.Name.ToString()}",
                ChangeSql = change.ToString()
            };
            RequestDialog(model);
        }
    }
}


