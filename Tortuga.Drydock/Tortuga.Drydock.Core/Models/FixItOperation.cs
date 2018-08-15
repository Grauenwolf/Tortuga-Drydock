using System.Windows.Input;
using Tortuga.Sails;

namespace Tortuga.Drydock.Models
{
    public abstract class FixItOperation : ViewModelBase
    {
        protected TableVM TableVM { get; }

        protected FixItOperation(TableVM tableVM)
        {
            TableVM = tableVM;
        }

        public ICommand FixItCommand => GetCommand(FixIt);

        public bool ShowFixIt { get => Get<bool>(); protected set => Set(value); }

        public void Refresh() => ShowFixIt = OnRefresh();

        protected abstract bool OnRefresh();

        private void FixIt() => TableVM.RequestDialog(OnFixIt());

        protected abstract FixItVM OnFixIt();

        public abstract string Title { get; }
        public abstract string ToolTip { get; }
    }
}