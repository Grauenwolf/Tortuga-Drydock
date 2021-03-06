using System.Data;
using System.Text;

namespace Tortuga.Drydock.Models.SqlServer
{
    public class FixDecimalAsText : FixItColumnOperation<SqlDbType>
    {
        private readonly SqlServerTableVM m_TableVM;

        public FixDecimalAsText(SqlServerTableVM tableVM, ColumnModel<SqlDbType> column) : base(tableVM, column)
        {
            m_TableVM = tableVM;
        }

        public override string Title => "Change to integer column";

        public override string ToolTip => "Generate script to change column type to int.";

        protected override FixItVM OnFixIt()
        {
            var change = new StringBuilder();
            var rollBack = new StringBuilder();

            change.AppendLine($"USE [{m_TableVM.DataSource.Name}]");
            rollBack.AppendLine($"USE [{m_TableVM.DataSource.Name}]");

            var column = Column;
            var nullText = column.IsNullable == true ? "NULL" : "NOT NULL";

            change.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} ALTER COLUMN {column.Column.QuotedSqlName} double {nullText}");
            rollBack.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} ALTER COLUMN {column.Column.QuotedSqlName} {column.Column.FullTypeName} {nullText}");

            var create = $"{m_TableVM.Table.Name.ToQuotedString()} {column.Column.QuotedSqlName} double {nullText}";

            return new FixItVM()
            {
                WindowTitle = $"Convert text column {column.Column.SqlName} to double in {m_TableVM.Table.Name.ToString()}",
                ChangeSql = change.ToString(),
                CreateSql = create,
                RollBackSql = rollBack.ToString()
            };
        }

        protected override bool OnRefresh()
        {
            return Column.TextContentFeatures?.HasFlag(TextContentFeatures.Decimal) == true;
        }
    }
}