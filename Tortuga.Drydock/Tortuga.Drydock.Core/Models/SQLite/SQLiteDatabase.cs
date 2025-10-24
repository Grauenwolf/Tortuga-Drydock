using System.Data;
using Tortuga.Chain;
using Tortuga.Chain.SQLite;

namespace Tortuga.Drydock.Models.SQLite;

public class SQLiteDatabase : DatabaseVM<SQLiteDataSource, SQLiteObjectName, DbType>
{
	public SQLiteDatabase(SQLiteDataSource dataSource) : base(dataSource)
	{
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

			Tables.AddRange(DataSource.DatabaseMetadata.GetTablesAndViews().Where(t => t.IsTable).OrderBy(t => t.Name.Name).Select(t => new SQLiteTableVM(DataSource, t)));
			Views.AddRange(DataSource.DatabaseMetadata.GetTablesAndViews().Where(t => !t.IsTable).OrderBy(t => t.Name.Name).Select(t => new ViewVM(t)));
		}
		finally
		{
			StopWork();
		}
	}
}