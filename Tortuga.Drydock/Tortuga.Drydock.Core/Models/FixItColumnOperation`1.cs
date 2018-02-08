namespace Tortuga.Drydock.Models
{
    public abstract class FixItColumnOperation<TDbType> : FixItOperation
    where TDbType : struct
    {
        public ColumnModel<TDbType> Column { get; }

        protected FixItColumnOperation(TableVM tableVM, ColumnModel<TDbType> column) : base(tableVM)
        {
            Column = column;
        }
    }
}



