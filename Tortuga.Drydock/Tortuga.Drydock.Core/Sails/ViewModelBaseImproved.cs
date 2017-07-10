using System;
using System.Runtime.CompilerServices;

namespace Tortuga.Sails
{
    public class ViewModelBaseImproved : ViewModelBase
    {
        /// <summary>
        /// Occurs when an Command throws an exception.
        /// </summary>
        public static event EventHandler<UnhandledViewModelExceptionEventArgs> UnhandledCommandEvent;


        /// <summary>
        /// Returns an ICommand wrapped around the provided action.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command">The command.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>DelegateCommand&lt;T&gt;.</returns>
        protected new DelegateCommand<T> GetCommand<T>(Action<T> command, [CallerMemberName] string propertyName = null)
        {
            Action<T> safeCommand = (p) =>
            {
                try
                {
                    command(p);
                }
                catch (Exception ex)
                {
                    var args = new UnhandledViewModelExceptionEventArgs(ex);
                    UnhandledCommandEvent?.Invoke(this, args);
                    if (!args.Handled)
                        throw;
                }
            };
            return base.GetCommand<T>(safeCommand, propertyName);
        }





        /// <summary>
        /// Returns an ICommand wrapped around the provided action.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>DelegateCommand&lt;System.Object&gt;.</returns>
        protected DelegateCommand<object> GetCommand(Action<object> command, [CallerMemberName] string propertyName = null)
        {
            Action<object> safeCommand = (p) =>
            {
                try
                {
                    command(p);
                }
                catch (Exception ex)
                {
                    var args = new UnhandledViewModelExceptionEventArgs(ex);
                    UnhandledCommandEvent?.Invoke(this, args);
                    if (!args.Handled)
                        throw;
                }
            };
            return base.GetCommand<object>(safeCommand, propertyName);
        }

        /// <summary>
        /// Returns an ICommand wrapped around the provided action.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>DelegateCommand.</returns>
        protected DelegateCommand GetCommand(Action command, [CallerMemberName] string propertyName = null)
        {
            Action safeCommand = () =>
            {
                try
                {
                    command();
                }
                catch (Exception ex)
                {
                    var args = new UnhandledViewModelExceptionEventArgs(ex);
                    UnhandledCommandEvent?.Invoke(this, args);
                    if (!args.Handled)
                        throw;
                }
            };
            return base.GetCommand(safeCommand, propertyName);
        }


        /// <summary>
        /// Returns an ICommand wrapped around the provided action.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command">The command.</param>
        /// <param name="canExecute">The can execute.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>DelegateCommand&lt;T&gt;.</returns>
        protected DelegateCommand<T> GetCommand<T>(Action<T> command, Func<T, bool> canExecute, [CallerMemberName] string propertyName = null)
        {
            Action<T> safeCommand = (p) =>
            {
                try
                {
                    command(p);
                }
                catch (Exception ex)
                {
                    var args = new UnhandledViewModelExceptionEventArgs(ex);
                    UnhandledCommandEvent?.Invoke(this, args);
                    if (!args.Handled)
                        throw;
                }
            };
            return base.GetCommand<T>(safeCommand, canExecute, propertyName);
        }

        /// <summary>
        /// Returns an ICommand wrapped around the provided action.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="canExecute">The can execute.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>DelegateCommand&lt;System.Object&gt;.</returns>
        protected DelegateCommand<object> GetCommand(Action<object> command, Func<object, bool> canExecute, [CallerMemberName] string propertyName = null)
        {
            Action<object> safeCommand = (p) =>
            {
                try
                {
                    command(p);
                }
                catch (Exception ex)
                {
                    var args = new UnhandledViewModelExceptionEventArgs(ex);
                    UnhandledCommandEvent?.Invoke(this, args);
                    if (!args.Handled)
                        throw;
                }
            };
            return base.GetCommand<object>(safeCommand, canExecute, propertyName);
        }

        /// <summary>
        /// Returns an ICommand wrapped around the provided action.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="canExecute">The can execute.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>DelegateCommand.</returns>
        protected DelegateCommand GetCommand(Action command, Func<bool> canExecute, [CallerMemberName] string propertyName = null)
        {
            Action safeCommand = () =>
            {
                try
                {
                    command();
                }
                catch (Exception ex)
                {
                    var args = new UnhandledViewModelExceptionEventArgs(ex);
                    UnhandledCommandEvent?.Invoke(this, args);
                    if (!args.Handled)
                        throw;
                }
            };
            return base.GetCommand(safeCommand, canExecute, propertyName);
        }


    }
}
