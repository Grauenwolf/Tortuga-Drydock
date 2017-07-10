using System;
using System.Windows;
using System.Windows.Input;

namespace Tortuga.Sails
{
    /// <summary>
    /// This represents an ICommand that isn't supported. It is used in abstract base classes where subclasses may optionally overide it.
    /// </summary>
    /// <seealso cref="System.Windows.Input.ICommand" />
    public class NotSupportedCommand : ICommand
    {
        public static NotSupportedCommand Value = new NotSupportedCommand();

        NotSupportedCommand() { }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { }
            remove { }
        }

        bool ICommand.CanExecute(object parameter) { return false; }

        void ICommand.Execute(object parameter) { }
    }
}
