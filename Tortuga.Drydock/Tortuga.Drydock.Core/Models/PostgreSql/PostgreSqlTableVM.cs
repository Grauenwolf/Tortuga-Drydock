using NpgsqlTypes;
using System;
using System.Threading.Tasks;
using Tortuga.Chain;
using Tortuga.Chain.Metadata;
using Tortuga.Chain.PostgreSql;

namespace Tortuga.Drydock.Models.PostgreSql
{
    public class PostgreSqlTableVM : TableVM<PostgreSqlObjectName, NpgsqlDbType>
    {
        public PostgreSqlTableVM(IClass1DataSource dataSource, TableOrViewMetadata<PostgreSqlObjectName, NpgsqlDbType> table) : base(dataSource, table)
        {

        }

        protected override Task AnalyzeColumnAsync(ColumnModel<NpgsqlDbType> column)
        {
            throw new NotImplementedException("Task-12 implement analyze columns");
        }


    }
}
