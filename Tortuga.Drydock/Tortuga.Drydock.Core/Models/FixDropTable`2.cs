using System.Text;

namespace Tortuga.Drydock.Models
{
    public class FixDropTable<TName, TDbType> : FixItOperation
        where TDbType : struct
    {
        private readonly TableVM<TName, TDbType> m_TableVM;
        public FixDropTable(TableVM<TName, TDbType> tableVM) : base(tableVM)
        {
            m_TableVM = tableVM;
        }

        public override string Title => "Unused Table";
        public override string ToolTip => "Generate script to drop unused table.";
        protected override bool OnRefresh() => m_TableVM.RowCount == 0;

        protected override FixItVM OnFixIt()
        {
            var verification = new StringBuilder();
            var change = new StringBuilder();

            change.AppendLine($"DROP TABLE {m_TableVM.QuotedTableName};");

            verification.AppendLine(m_TableVM.DataSource.From(m_TableVM.QuotedTableName).AsCount().CommandText());

            return new FixItVM()
            {
                WindowTitle = $"Unused Table",
                VerificationSql = verification.ToString(),
                ChangeSql = change.ToString()
            };
        }
    }
}


