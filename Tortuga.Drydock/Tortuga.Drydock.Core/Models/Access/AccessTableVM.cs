using System.Data;
using System.Data.OleDb;
using Tortuga.Chain.Access;
using Tortuga.Chain.DataSources;
using Tortuga.Chain.Metadata;

namespace Tortuga.Drydock.Models.Access;

public class AccessTableVM : TableVM<AccessObjectName, OleDbType>
{
    public AccessTableVM(ICrudDataSource dataSource, TableOrViewMetadata<AccessObjectName, OleDbType> table) : base(dataSource, table)
    {
    }

    public override string QuotedTableName => Table.Name.ToQuotedString();

    protected override Task AnalyzeColumnAsync(ColumnModel<OleDbType> column)
    {
        throw new NotImplementedException("Task-12 implement analyze columns");
    }

    protected override Task<DataTable> OnShowTopTenAsync(ColumnModel<OleDbType> column, int rowCount)
    {
        throw new NotImplementedException();
    }
}