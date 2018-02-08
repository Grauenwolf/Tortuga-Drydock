using System.Data;

namespace Tortuga.Drydock.Models.SqlServer
{
    public class FixDomainUserNameCheckConstraint : FixItColumnOperation<SqlDbType>
    {
        private readonly SqlServerTableVM m_TableVM;

        public FixDomainUserNameCheckConstraint(SqlServerTableVM tableVM, ColumnModel<SqlDbType> column) : base(tableVM, column)
        {
            m_TableVM = tableVM;
        }

        public override string Title => "Add domain user name check constraint";

        public override string ToolTip => "Generate script to add a check constraint ensuring domain user name-like values.";

        protected override FixItVM OnFixIt()
        {
            var change = $"ALTER TABLE {m_TableVM.QuotedTableName} ADD CONSTRAINT C_{m_TableVM.Table.Name.Name}_{Column.Column.ClrName} CHECK (charindex('\\',{Column.Column.QuotedSqlName})> 0 );";
            var rollBack = $"ALTER TABLE {m_TableVM.QuotedTableName} DROP CONSTRAINT C_{m_TableVM.Table.Name.Name}_{Column.Column.ClrName}";

            return new FixItVM()
            {
                WindowTitle = $"Create check constraint for {m_TableVM.Table.Name.ToString()}",
                ChangeSql = change,
                RollBackSql = rollBack
            };

        }

        protected override bool OnRefresh()
        {
            return Column.TextContentFeatures.HasValue && Column.TextContentFeatures.Value.HasFlag(TextContentFeatures.DomainUserName) && Column.HasCheckConstraint == false;
        }
    }
}

