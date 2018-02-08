using System.Text;

namespace Tortuga.Drydock.Models
{
    public class FixDropTable<TName, TDbType> : FixItOperation
        where TDbType : struct
    {
        public FixDropTable(TableVM<TName, TDbType> tableVM) : base(tableVM)
        {
        }

        public override string Title => "Unused Table";
        public override string ToolTip => "Generate script to drop unused table.";
        protected override bool OnRefresh() => TableVM.RowCount == 0;

        protected override FixItVM OnFixIt()
        {
            var verification = new StringBuilder();
            var change = new StringBuilder();

            change.AppendLine($"DROP TABLE {TableVM.QuotedTableName};");

            verification.AppendLine(TableVM.DataSource.From(TableVM.QuotedTableName).AsCount().CommandText());

            return new FixItVM()
            {
                WindowTitle = $"Unused Table",
                VerificationSql = verification.ToString(),
                ChangeSql = change.ToString()
            };
        }
    }
}


