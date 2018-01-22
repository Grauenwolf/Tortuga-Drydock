using NpgsqlTypes;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tortuga.Chain;
using Tortuga.Chain.Metadata;
using Tortuga.Chain.PostgreSql;

namespace Tortuga.Drydock.Models.PostgreSql
{
    public class PostgreSqlTableVM : TableVM<PostgreSqlObjectName, NpgsqlDbType>
    {
        public PostgreSqlTableVM(IClass1DataSource dataSource, TableOrViewMetadata<PostgreSqlObjectName, NpgsqlDbType> table) : base(dataSource, table)
        {

        }

        protected override Task AnalyzeColumnAsync(ColumnModel<NpgsqlDbType> column)
        {
            throw new NotImplementedException("Task-12 implement analyze columns");
        }

        public override bool SupportsFixNull => true;


        protected override void FixNull()
        {
            var verification = new StringBuilder();
            var change = new StringBuilder();
            var rollBack = new StringBuilder();

            var afectedColumns = Columns.Where(c => c.IsNullable && c.NullCount == 0).ToList();

            foreach (var column in afectedColumns)
            {
                change.AppendLine($"ALTER TABLE {Table.Name.ToQuotedString()} ALTER COLUMN {column.Column.QuotedSqlName} SET NOT NULL");
                rollBack.AppendLine($"ALTER TABLE {Table.Name.ToQuotedString()} ALTER COLUMN {column.Column.QuotedSqlName} SET NULL");
            }

            verification.AppendLine($"SELECT * FROM {Table.Name.ToQuotedString()} WHERE " + string.Join(" OR ", afectedColumns.Select(x => $"{x.Column.QuotedSqlName} IS NULL")));

            var model = new FixItVM()
            {
                WindowTitle = $"Nullable columns without nulls for {Table.Name.ToString()}",
                VerificationSql = verification.ToString(),
                ChangeSql = change.ToString(),
                RollBackSql = rollBack.ToString()
            };
            RequestDialog(model);
        }
    }
}
