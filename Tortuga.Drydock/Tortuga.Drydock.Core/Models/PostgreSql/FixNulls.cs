using System.Data;
using System.Linq;
using System.Text;

namespace Tortuga.Drydock.Models.PostgreSql
{
    public class FixNulls : FixItOperation
    {
        private readonly PostgreSqlTableVM m_TableVM;

        public FixNulls(PostgreSqlTableVM tableVM) : base(tableVM)
        {
            m_TableVM = tableVM;
        }

        public override string Title => "Nullable Columns";
        public override string ToolTip => "Generate script to mark columns as non-nullable.";

        protected override bool OnRefresh() => m_TableVM.Columns.Any(x => x.NullCount == 0);

        protected override FixItVM OnFixIt()
        {
            var verification = new StringBuilder();
            var change = new StringBuilder();
            var rollBack = new StringBuilder();

            var afectedColumns = m_TableVM.Columns.Where(c => c.IsNullable == true && c.NullCount == 0).ToList();

            foreach (var column in afectedColumns)
            {
                change.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} ALTER COLUMN {column.Column.QuotedSqlName} SET NOT NULL");
                rollBack.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} ALTER COLUMN {column.Column.QuotedSqlName} SET NULL");
            }

            verification.AppendLine($"SELECT * FROM {m_TableVM.Table.Name.ToQuotedString()} WHERE " + string.Join(" OR ", afectedColumns.Select(x => $"{x.Column.QuotedSqlName} IS NULL")));

            return new FixItVM()
            {
                WindowTitle = $"Nullable columns without nulls for {m_TableVM.Table.Name.ToString()}",
                VerificationSql = verification.ToString(),
                ChangeSql = change.ToString(),
                RollBackSql = rollBack.ToString()
            };
        }
    }
}