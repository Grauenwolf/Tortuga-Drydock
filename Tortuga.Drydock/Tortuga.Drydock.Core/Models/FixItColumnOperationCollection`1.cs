using Tortuga.Anchor.Collections;

namespace Tortuga.Drydock.Models
{
    public class FixItColumnOperationCollection<TDbType> : ObservableCollectionExtended<FixItColumnOperation<TDbType>> where TDbType : struct
    {
        public void RefreshAll()
        {
            foreach (var item in this)
                item.Refresh();
        }
    }
}


