using System;
using System.Data;
using System.Threading.Tasks;
using Tortuga.Chain;
using Tortuga.Chain.Metadata;
using Tortuga.Chain.SQLite;

namespace Tortuga.Drydock.Models.SQLite
{
    public class SQLiteTableVM : TableVM<SQLiteObjectName, DbType>
    {
        public SQLiteTableVM(IClass1DataSource dataSource, TableOrViewMetadata<SQLiteObjectName, DbType> table) : base(dataSource, table)
        {

        }
        public override string QuotedTableName => Table.Name.ToQuotedString();

        protected override Task AnalyzeColumnAsync(ColumnModel<DbType> column)
        {
            throw new NotImplementedException("Task-12 implement analyze columns");
        }

        protected override Task<DataTable> OnShowTopTenAsync(ColumnModel<DbType> column, int rowCount)
        {
            throw new NotImplementedException();
        }
    }
}
