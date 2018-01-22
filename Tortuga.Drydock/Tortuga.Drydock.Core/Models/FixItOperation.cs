using System.Windows.Input;
using Tortuga.Sails;

namespace Tortuga.Drydock.Models
{
    public abstract class FixItOperation : ViewModelBaseImproved
    {
        private readonly TableVM m_TableVM;

        protected FixItOperation(TableVM tableVM)
        {
            m_TableVM = tableVM;
        }

        public ICommand FixItCommand => GetCommand(FixIt);

        public bool ShowFixIt { get => Get<bool>(); protected set => Set(value); }

        public abstract void Refresh();

        void FixIt()
        {
            m_TableVM.RequestDialog(OnFixIt());
        }

        protected abstract FixItVM OnFixIt();

        public abstract string Title { get; }
        public abstract string ToolTip { get; }

    }
}



