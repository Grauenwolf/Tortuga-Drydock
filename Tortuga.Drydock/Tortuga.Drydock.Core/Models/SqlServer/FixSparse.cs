using System.Data;
using System.Linq;
using System.Text;

namespace Tortuga.Drydock.Models.SqlServer
{
    public class FixSparse : FixItOperation
    {
        private readonly SqlServerTableVM m_TableVM;

        public FixSparse(SqlServerTableVM tableVM) : base(tableVM)
        {
            m_TableVM = tableVM;
        }

        public override string Title => "Sparse Columns";

        public override string ToolTip => "Generate script to mark columns as sparse or non-sparse as appropriate.";

        protected override bool OnRefresh() => m_TableVM.Columns.Cast<SqlServerColumnModel>().Any(x => x.SparseCandidate || x.SparseWarning);

        protected override FixItVM OnFixIt()
        {
            var change = new StringBuilder();
            change.AppendLine($"USE [{m_TableVM.DataSource.Name}]"); //Task-25 replace this with something more reliable.

            var rollBack = new StringBuilder();
            rollBack.AppendLine($"USE [{m_TableVM.DataSource.Name}]");


            foreach (var column in m_TableVM.Columns.Cast<SqlServerColumnModel>().Where(c => c.SparseCandidate))
            {
                change.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} ALTER COLUMN {column.Column.QuotedSqlName} {column.Column.FullTypeName} SPARSE");
                rollBack.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} ALTER COLUMN {column.Column.QuotedSqlName} {column.Column.FullTypeName} NULL");
            }
            foreach (var column in m_TableVM.Columns.Cast<SqlServerColumnModel>().Where(c => c.SparseWarning))
            {
                change.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} ALTER COLUMN {column.Column.QuotedSqlName} {column.Column.FullTypeName} NULL");
                rollBack.AppendLine($"ALTER TABLE {m_TableVM.Table.Name.ToQuotedString()} ALTER COLUMN {column.Column.QuotedSqlName} {column.Column.FullTypeName} SPARSE");
            }

            return new FixItVM()
            {
                WindowTitle = $"Sparse columns for {m_TableVM.Table.Name.ToString()}",
                ChangeSql = change.ToString(),
                RollBackSql = rollBack.ToString()
            };

        }

    }
}


