using Tortuga.Anchor.Modeling;

namespace Tortuga.Drydock.Models
{
    public class ColumnCollection<TDbType> : ModelCollection<ColumnModel<TDbType>>
        where TDbType : struct
    {

        public TableVM TableVM
        {
            get { return Get<TableVM>(); }
            set
            {
                if (Set(value))
                {
                    foreach (var row in this)
                        row.TableVM = value;
                }
            }
        }

        protected override void OnItemAdded(ColumnModel<TDbType> item)
        {
            base.OnItemAdded(item);
            item.TableVM = TableVM;
        }


    }
}


