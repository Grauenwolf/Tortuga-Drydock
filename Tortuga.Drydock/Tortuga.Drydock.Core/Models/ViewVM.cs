using System;
using Tortuga.Anchor.Modeling;
using Tortuga.Chain.Metadata;

namespace Tortuga.Drydock.Models
{
    public class ViewVM : ModelBase
    {
        readonly TableOrViewMetadata m_View;


        public ViewVM(TableOrViewMetadata view)
        {
            if (view.IsTable)
                throw new ArgumentException($"{view.Name} is not a view.", nameof(view));
            m_View = view;
        }

        public TableOrViewMetadata View
        {
            get => m_View;
        }
    }
}
