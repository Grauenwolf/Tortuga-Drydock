using System;
using System.Data.OleDb;
using System.Threading.Tasks;
using Tortuga.Chain;
using Tortuga.Chain.Access;
using Tortuga.Chain.Metadata;

namespace Tortuga.Drydock.Models.Access
{
    public class AccessTableVM : TableVM<AccessObjectName, OleDbType>
    {
        public AccessTableVM(IClass1DataSource dataSource, TableOrViewMetadata<AccessObjectName, OleDbType> table) : base(dataSource, table)
        {

        }

        protected override Task AnalyzeColumnAsync(ColumnModel<OleDbType> column)
        {
            throw new NotImplementedException("Task-12 implement analyze columns");
        }

        protected override void FixNull()
        {
            throw new NotImplementedException();
        }
    }
}
