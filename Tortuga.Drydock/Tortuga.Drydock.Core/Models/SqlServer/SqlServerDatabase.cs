using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Tortuga.Chain;
using Tortuga.Chain.SqlServer;

namespace Tortuga.Drydock.Models.SqlServer
{
    /// <summary>
    /// This represents a database connection and any cached data about that database.
    /// This class is specialized for SQL Server.
    /// </summary>
    /// <seealso cref="Tortuga.Drydock.Models.Database{Tortuga.Chain.SqlServerDataSource}" />
    public class SqlServerDatabase : DatabaseVM<SqlServerDataSource, SqlServerObjectName, SqlDbType>
    {
        public SqlServerDatabase(SqlServerDataSource dataSource)
        {
            DataSource = dataSource;
        }

        public override sealed async Task LoadSchemaAsync()
        {
            StartWork();
            try
            {

                await (Task.Run(() =>
                {
                    DataSource.DatabaseMetadata.PreloadTables();
                    DataSource.DatabaseMetadata.PreloadViews();
                })); //Task-10, we need an async version of this. 


                Tables.AddRange(DataSource.DatabaseMetadata.GetTablesAndViews().Where(t => t.IsTable).OrderBy(t => t.Name.Schema).ThenBy(t => t.Name.Name).Select(t => new SqlServerTableVM(DataSource, (SqlServerTableOrViewMetadata<SqlDbType>)t)));
                Views.AddRange(DataSource.DatabaseMetadata.GetTablesAndViews().Where(t => !t.IsTable).OrderBy(t => t.Name.Schema).ThenBy(t => t.Name.Name).Select(t => new ViewVM(t)));
            }
            finally
            {
                StopWork();
            }

        }




        async public override Task PreliminaryAnalysisAsync(TableVM table)
        {

            var typedTable = (SqlServerTableVM)table;

            const string sql = @"SELECT S.name AS [Schema],
       T.name AS Name,
       CONVERT(   BIT,
                  CASE
                      WHEN NOT EXISTS
                               (
                                   SELECT *
                                   FROM sys.indexes AS i
                                   WHERE i.object_id = T.object_id
                                         AND type = 1
                               ) THEN
                          1
                      ELSE
                          0
                  END
              ) AS IsHeap,
       (
           SELECT COUNT(*)
           FROM sys.indexes AS i
           WHERE i.object_id = T.object_id
                 AND type <> 0
       ) AS IndexCount,
       (
           SELECT SUM(p.rows)
           FROM sys.partitions AS p
           WHERE p.index_id < 2
                 AND p.object_id = T.object_id
       ) AS [RowCount],
       ROW_NUMBER() OVER (ORDER BY S.name, T.name) AS SortIndex,
       T.object_id AS ObjectId
FROM sys.tables AS T
    INNER JOIN sys.schemas AS S
        ON T.schema_id = S.schema_id
WHERE T.name = @Name
      AND S.name = @Schema
ORDER BY S.name,
         T.name;";

            var tableData = await DataSource.Sql(sql, new { typedTable.Table.Name.Schema, typedTable.Table.Name.Name }).ToObject<TableData>().ExecuteAsync();
            typedTable.ObjectId = tableData.ObjectId;
            typedTable.RowCount = tableData.RowCount;
            typedTable.IndexCount = tableData.IndexCount;
            typedTable.IsHeap = tableData.IsHeap;

            table.FixItOperations.RefreshAll();
        }



        async public override Task PreliminaryAnalysisAsync()
        {
            const string sql = @"SELECT S.name AS [Schema],
       T.name AS Name,
       CONVERT(   BIT,
                  CASE
                      WHEN NOT EXISTS
                               (
                                   SELECT *
                                   FROM sys.indexes AS i
                                   WHERE i.object_id = T.object_id
                                         AND type = 1
                               ) THEN
                          1
                      ELSE
                          0
                  END
              ) AS IsHeap,
       (
           SELECT COUNT(*)
           FROM sys.indexes AS i
           WHERE i.object_id = T.object_id
                 AND type <> 0
       ) AS IndexCount,
       (
           SELECT SUM(p.rows)
           FROM sys.partitions AS p
           WHERE p.index_id < 2
                 AND p.object_id = T.object_id
       ) AS [RowCount],
       ROW_NUMBER() OVER (ORDER BY S.name, T.name) AS SortIndex,
       T.object_id AS ObjectId
FROM sys.tables AS T
    INNER JOIN sys.schemas AS S
        ON T.schema_id = S.schema_id
ORDER BY S.name,
         T.name;";

            var data = await DataSource.Sql(sql).ToCollection<TableData>().ExecuteAsync();
            foreach (SqlServerTableVM table in Tables)
            {
                foreach (var tableData in data)
                {
                    if (tableData.Schema == table.Table.Name.Schema && tableData.Name == table.Table.Name.Name)
                    {
                        table.ObjectId = tableData.ObjectId;
                        table.RowCount = tableData.RowCount;
                        table.IndexCount = tableData.IndexCount;
                        table.IsHeap = tableData.IsHeap;
                        table.FixItOperations.RefreshAll();
                        table.SortIndex = tableData.SortIndex;
                        break;
                    }
                }



            }

        }

        class TableData
        {
            public string Name { get; set; }
            public string Schema { get; set; }
            public int ColumnCount { get; set; }
            public bool IsHeap { get; set; }
            public int IndexCount { get; set; }
            public long ObjectId { get; set; }
            public long RowCount { get; set; }
            public int SortIndex { get; set; }
        }
    }

}

