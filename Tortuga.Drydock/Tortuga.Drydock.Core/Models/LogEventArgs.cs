using System;

namespace Tortuga.Drydock.Models
{
    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
