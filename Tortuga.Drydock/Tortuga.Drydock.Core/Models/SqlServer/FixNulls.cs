using System.Data;
using System.Linq;
using System.Text;

namespace Tortuga.Drydock.Models.SqlServer
{
    public class FixNulls : FixItOperation
    {
        private readonly SqlServerTableVM m_TableVM;
        public FixNulls(SqlServerTableVM tableVM) : base(tableVM)
        {
            m_TableVM = tableVM;
        }

        public override string Title => "Nullable Columns";
        public override string ToolTip => "Generate script to mark columns as non-nullable.";
        public override void Refresh()
        {
            ShowFixIt = m_TableVM.Columns.Any(x => x.NullCount == 0);
        }

        protected override FixItVM OnFixIt()
        {
            var verification = new StringBuilder();
            var change = new StringBuilder();
            var rollBack = new StringBuilder();

            verification.AppendLine($"USE [{m_TableVM.DataSource.Name}]");
            change.AppendLine($"USE [{m_TableVM.DataSource.Name}]");
            rollBack.AppendLine($"USE [{m_TableVM.DataSource.Name}]");

            var afectedColumns = m_TableVM.Columns.Where(c => c.IsNullable && c.NullCount == 0).Cast<SqlServerColumnModel>().ToList();

            verification.AppendLine($"SELECT * FROM {m_TableVM.Table.Name.ToQuotedString()} WHERE " + string.Join(" OR ", afectedColumns.Select(x => $"{x.Column.QuotedSqlName} IS NULL")));

            foreach (var column in afectedColumns)
            {
                change.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} ALTER COLUMN {column.Column.QuotedSqlName} {column.Column.FullTypeName} NOT NULL");

                if (column.IsSparse)
                    rollBack.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} ALTER COLUMN {column.Column.QuotedSqlName} {column.Column.FullTypeName} SPARSE");
                else
                    rollBack.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} ALTER COLUMN {column.Column.QuotedSqlName} {column.Column.FullTypeName} NULL");
            }

            return new FixItVM()
            {
                WindowTitle = $"Nullable columns without nulls for {m_TableVM.Table.Name.ToString()}",
                ChangeSql = change.ToString(),
                RollBackSql = rollBack.ToString(),
                VerificationSql = verification.ToString()
            };
        }
    }
}


