using System.Linq;
using System.Text;

namespace Tortuga.Drydock.Models.SqlServer
{
    public class FixObsoleteTypes : FixItOperation
    {
        private readonly SqlServerTableVM m_TableVM;

        public FixObsoleteTypes(SqlServerTableVM tableVM) : base(tableVM)
        {
            m_TableVM = tableVM;
        }

        public override string Title => "Obsolete Types";
        public override string ToolTip => "Generates a script to correct obsolete data types";

        protected override bool OnRefresh() => m_TableVM.Columns.Any(x => x.ObsoleteReplaceType != null);

        protected override FixItVM OnFixIt()
        {
            var change = new StringBuilder();
            var rollBack = new StringBuilder();

            change.AppendLine($"USE [{m_TableVM.DataSource.Name}]");
            rollBack.AppendLine($"USE [{m_TableVM.DataSource.Name}]");

            var afectedColumns = m_TableVM.Columns.Where(c => c.ObsoleteReplaceType != null).Cast<SqlServerColumnModel>().ToList();

            foreach (var column in afectedColumns)
            {
                var nullText = column.IsNullable ? "NULL" : "NOT NULL";

                change.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} ALTER COLUMN {column.Column.QuotedSqlName} {column.ObsoleteReplaceType} {nullText}");

                rollBack.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} ALTER COLUMN {column.Column.QuotedSqlName} {column.Column.FullTypeName} {nullText}");
            }

            return new FixItVM()
            {
                WindowTitle = $"Obsolete columns in {m_TableVM.Table.Name.ToString()}",
                ChangeSql = change.ToString(),
                RollBackSql = rollBack.ToString()
            };
        }
    }
}


