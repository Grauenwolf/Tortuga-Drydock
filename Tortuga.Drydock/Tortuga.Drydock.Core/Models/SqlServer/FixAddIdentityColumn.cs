using System.Data;
using System.Linq;
using System.Text;

namespace Tortuga.Drydock.Models.SqlServer
{
    public class FixAddIdentityColumn : FixItOperation
    {
        private readonly SqlServerTableVM m_TableVM;

        public FixAddIdentityColumn(SqlServerTableVM tableVM) : base(tableVM)
        {
            m_TableVM = tableVM;
        }

        public override string Title => "Add Identity Column";
        public override string ToolTip => "Generate script to add an identity column and mark it as the primary key.";



        protected override bool OnRefresh() => m_TableVM.IsHeap == true;

        protected override FixItVM OnFixIt()
        {
            var identityColumn = m_TableVM.Columns.Where(x => x.IsIdentity).Select(x => x.Column.QuotedSqlName).FirstOrDefault();
            var addColumn = false;
            if (identityColumn == null)
            {
                identityColumn = "[Id]";
                addColumn = true;
            }

            var change = new StringBuilder();
            change.AppendLine($"USE [{m_TableVM.DataSource.Name}]");
            if (addColumn)
                change.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} ADD {identityColumn} INT NOT NULL IDENTITY");
            change.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} ADD CONSTRAINT PK_{m_TableVM.Table.Name.Name} PRIMARY KEY ({identityColumn})");

            var rollBack = new StringBuilder();
            rollBack.AppendLine($"USE [{m_TableVM.DataSource.Name}]");
            rollBack.AppendLine($"DROP INDEX PK_{m_TableVM.Table.Name.Name} ON {m_TableVM.Table.Name.ToQuotedString()}");
            if (addColumn)
                rollBack.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} DROP COLUMN {identityColumn}");

            return new FixItVM()
            {
                WindowTitle = $"Create identity column for {m_TableVM.Table.Name.ToString()}",
                ChangeSql = change.ToString(),
                RollBackSql = rollBack.ToString()
            };
        }
    }
}


