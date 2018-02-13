using System.Data;
using System.Text;

namespace Tortuga.Drydock.Models.SqlServer
{
    public class FixEmailCheckConstraint : FixItColumnOperation<SqlDbType>
    {
        private readonly SqlServerTableVM m_TableVM;

        public FixEmailCheckConstraint(SqlServerTableVM tableVM, ColumnModel<SqlDbType> column) : base(tableVM, column)
        {
            m_TableVM = tableVM;
        }

        public override string Title => "Add email check constraint";

        public override string ToolTip => "Generate script to add a check constraint ensuring email address-like values.";

        protected override FixItVM OnFixIt()
        {
            var change = new StringBuilder();
            var rollBack = new StringBuilder();

            change.AppendLine($"USE [{m_TableVM.DataSource.Name}]");
            rollBack.AppendLine($"USE [{m_TableVM.DataSource.Name}]");

            var create = $"CONSTRAINT C_{m_TableVM.Table.Name.Name}_{Column.Column.ClrName} CHECK (charindex('@',{Column.Column.QuotedSqlName})> 0 )";
            change.AppendLine($"ALTER TABLE {m_TableVM.QuotedTableName} ADD CONSTRAINT C_{m_TableVM.Table.Name.Name}_{Column.Column.ClrName} CHECK (charindex('@',{Column.Column.QuotedSqlName})> 0 );");
            rollBack.AppendLine($"ALTER TABLE {m_TableVM.QuotedTableName} DROP CONSTRAINT C_{m_TableVM.Table.Name.Name}_{Column.Column.ClrName}");

            return new FixItVM()
            {
                WindowTitle = $"Create check constraint for {m_TableVM.Table.Name.ToString()}",
                ChangeSql = change.ToString(),
                RollBackSql = rollBack.ToString(),
                CreateSql = create
            };

        }

        protected override bool OnRefresh()
        {
            return Column.TextContentFeatures.HasValue && Column.TextContentFeatures.Value.HasFlag(TextContentFeatures.EmailAddress) && Column.HasCheckConstraint == false;
        }
    }
}


