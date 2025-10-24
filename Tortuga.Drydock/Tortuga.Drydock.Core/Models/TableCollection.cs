using Tortuga.Anchor.Modeling;

namespace Tortuga.Drydock.Models;

public class TableCollection<TName, TDbType> : ModelCollection<TableVM<TName, TDbType>>
	where TName : struct
	where TDbType : struct
{
}