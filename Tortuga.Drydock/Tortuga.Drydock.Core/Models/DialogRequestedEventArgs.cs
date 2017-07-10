using System;

namespace Tortuga.Drydock.Models
{
    public class DialogRequestedEventArgs : EventArgs
    {
        public DialogRequestedEventArgs(object dataContext)
        {
            DataContext = dataContext;
        }

        public object DataContext { get; }
    }
}


