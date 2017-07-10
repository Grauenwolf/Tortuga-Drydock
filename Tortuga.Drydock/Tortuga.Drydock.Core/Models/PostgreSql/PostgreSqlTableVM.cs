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
            var change = new StringBuilder();
            change.AppendLine($"USE [{DataSource.Name}]");
            foreach (var column in Columns.Where(c => c.IsNullable && c.NullCount == 0))
            {
                change.AppendLine($"ALTER TABLE {Table.Name.ToQuotedString()} ALTER COLUMN [{column.Name}] SET NOT NULL");
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
