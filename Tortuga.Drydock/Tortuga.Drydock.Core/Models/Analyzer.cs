using System.Windows.Input;

namespace Tortuga.Drydock.Models
{

    /// <summary>
    /// This represents a database specific analyzer.
    /// </summary>
    public abstract class Analyzer
    {
        public abstract ICommand Command { get; }
        public abstract string Name { get; }
    }
}
