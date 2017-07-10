using System;

namespace Tortuga.Sails
{
    /// <summary>
    /// Class UnhandledViewModelExceptionEventArgs.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class UnhandledViewModelExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnhandledViewModelExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public UnhandledViewModelExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="UnhandledViewModelExceptionEventArgs"/> has been handled.
        /// </summary>
        /// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
        public bool Handled { get; set; }

    }
}
