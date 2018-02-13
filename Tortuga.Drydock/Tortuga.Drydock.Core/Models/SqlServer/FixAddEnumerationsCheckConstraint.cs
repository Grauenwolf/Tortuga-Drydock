using System.Data;
using System.Linq;
using System.Text;

namespace Tortuga.Drydock.Models.SqlServer
{
    public class FixAddEnumerationsCheckConstraint : FixItColumnOperation<SqlDbType>
    {
        private readonly SqlServerTableVM m_TableVM;

        public FixAddEnumerationsCheckConstraint(SqlServerTableVM tableVM, ColumnModel<SqlDbType> column) : base(tableVM, column)
        {
            m_TableVM = tableVM;
        }

        public override string Title => "Add enumeration check constraint";

        public override string ToolTip => "Generate script to add a check constraint ensuring the value is one of a finite list.";

        protected override FixItVM OnFixIt()
        {
            var change = new StringBuilder();
            var rollBack = new StringBuilder();

            change.AppendLine($"USE [{m_TableVM.DataSource.Name}]");
            rollBack.AppendLine($"USE [{m_TableVM.DataSource.Name}]");

            var checkExpression = Column.Column.QuotedSqlName + " IN (" + string.Join(", ", Column.TopNValues.Where(x => x != null).Select(x => "'" + x.ToString().Replace("'", "''") + "'")) + ")";

            var create = $"CONSTRAINT C_{m_TableVM.Table.Name.Name}_{Column.Column.ClrName} CHECK ({checkExpression})";
            change.AppendLine($"ALTER TABLE {m_TableVM.QuotedTableName} ADD CONSTRAINT C_{m_TableVM.Table.Name.Name}_{Column.Column.ClrName} CHECK ({checkExpression});");
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
            return Column.HasForiegnKeyConstraint == false && Column.HasCheckConstraint == false && Column.TopNValues.Count <= 10 && Column.TopNValues.Count > 0;
        }
    }
}


