using NpgsqlTypes;
using System;
using System.Data;
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
            FixItOperations.Add(new FixNulls(this));
        }

        public override string QuotedTableName => Table.Name.ToQuotedString();
        protected override Task AnalyzeColumnAsync(ColumnModel<NpgsqlDbType> column)
        {
            throw new NotImplementedException("Task-12 implement analyze columns");
        }

        protected override Task<DataTable> OnShowTopTenAsync(ColumnModel<NpgsqlDbType> column)
        {
            throw new NotImplementedException();
        }
    }
}
