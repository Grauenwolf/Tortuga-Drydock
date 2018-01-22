using System;
using System.Threading.Tasks;
using Tortuga.Chain;

namespace Tortuga.Drydock.Models
{
    /// <summary>
    /// This represents a database connection and any cached data about that database.
    /// </summary>
    /// <typeparam name="TDataSource"></typeparam>
    /// <seealso cref="Tortuga.Drydock.Models.DatabaseVM" />
    public abstract class DatabaseVM<TDataSource, TName, TDbType> : DatabaseVM
        where TDataSource : IClass1DataSource
        where TDbType : struct

    {
        public new TDataSource DataSource { get; protected set; }

        //public sealed override IDatabaseMetadataCache DatabaseMetadata
        //{
        //    get => DataSource.DatabaseMetadata;
        //}

        public TableCollection<TName, TDbType> Tables { get => GetNew<TableCollection<TName, TDbType>>(); }

        public ViewCollection Views { get => GetNew<ViewCollection>(); }

        protected sealed override IClass1DataSource GetDataSource() => DataSource;

        /// <summary>
        /// Attaches the UI events to the view models.
        /// </summary>
        /// <param name="dialogRequestedEventHandler">The dialog requested event handler.</param>
        public override void AttachUIEvents(EventHandler<DialogRequestedEventArgs> dialogRequestedEventHandler, EventHandler<LogEventArgs> logEventHandler)
        {
            foreach (var table in Tables)
            {
                table.DialogRequested += dialogRequestedEventHandler;
                table.LogEvent += logEventHandler;
            }
        }

        public override async Task PreliminaryAnalysisAsync()
        {
            foreach (var table in Tables)
            {
                await PreliminaryAnalysisAsync(table);
            }
        }

        public override async Task PreliminaryAnalysisAsync(TableVM table)
        {
            table.RowCount = await DataSource.From(table.FullName).AsCount().ExecuteAsync();
        }
    }
}
