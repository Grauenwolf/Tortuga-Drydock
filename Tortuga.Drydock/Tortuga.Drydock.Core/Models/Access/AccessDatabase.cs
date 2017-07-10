using System.Data.OleDb;
using System.Linq;
using System.Threading.Tasks;
using Tortuga.Chain;
using Tortuga.Chain.Access;

namespace Tortuga.Drydock.Models.Access
{
    public class AccessDatabase : DatabaseVM<AccessDataSource, AccessObjectName, OleDbType>
    {
        public AccessDatabase(AccessDataSource dataSource)
        {
            DataSource = dataSource;
        }

        public override Task PreliminaryAnalysisAsync()
        {
            return Task.CompletedTask;
        }

        public override sealed async Task LoadSchemaAsync()
        {
            StartWork();
            try
            {

                await (Task.Run(() => DataSource.DatabaseMetadata.Preload())); //Task-10, we need an async version of this. 


                Tables.AddRange(DataSource.DatabaseMetadata.GetTablesAndViews().Where(t => t.IsTable).OrderBy(t => t.Name.Name).Select(t => new AccessTableVM(DataSource, t)));
                Views.AddRange(DataSource.DatabaseMetadata.GetTablesAndViews().Where(t => !t.IsTable).OrderBy(t => t.Name.Name).Select(t => new ViewVM(t)));
            }
            finally
            {
                StopWork();
            }

        }

        public override Task PreliminaryAnalysisAsync(TableVM table)
        {
            return Task.CompletedTask;
        }
    }
}
