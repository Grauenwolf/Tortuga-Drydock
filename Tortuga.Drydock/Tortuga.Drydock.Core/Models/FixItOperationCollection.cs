using Tortuga.Anchor.Collections;

namespace Tortuga.Drydock.Models
{
    public class FixItOperationCollection : ObservableCollectionExtended<FixItOperation>
    {
        public void RefreshAll()
        {
            foreach (var item in this)
                item.Refresh();
        }
    }
}


