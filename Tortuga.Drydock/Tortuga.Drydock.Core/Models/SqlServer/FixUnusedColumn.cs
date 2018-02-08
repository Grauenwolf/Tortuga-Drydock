using System.Data;

namespace Tortuga.Drydock.Models.SqlServer
{
    public class FixUnusedColumn : FixItColumnOperation<SqlDbType>
    {
        private readonly SqlServerTableVM m_TableVM;

        public FixUnusedColumn(SqlServerTableVM tableVM, ColumnModel<SqlDbType> column) : base(tableVM, column)
        {
            m_TableVM = tableVM;
        }

        public override string Title => "Drop unused column";

        public override string ToolTip => "Generate script to drop the unused column.";

        protected override FixItVM OnFixIt()
        {
            var verification = $"SELECT DISTINCT {Column.Column.QuotedSqlName} FROM {m_TableVM.QuotedTableName};";
            var change = $"ALTER TABLE {m_TableVM.QuotedTableName} DROP {Column.Column.QuotedSqlName};";

            return new FixItVM()
            {
                WindowTitle = $"Create check constraint for {m_TableVM.Table.Name.ToString()}",
                VerificationSql = verification,
                ChangeSql = change,
            };

        }

        protected override bool OnRefresh()
        {
            return Column.NoDistinctValues == true || Column.NullRate == 1;
        }
    }
}


