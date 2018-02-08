using System;
using Tortuga.Chain;
using Tortuga.Sails;

namespace Tortuga.Drydock.Models
{
    public abstract class TableVM : ViewModelBaseImproved
    {
        int m_WorkCount;

        protected TableVM(IClass1DataSource dataSource)
        {
            DataSource = dataSource;
        }
        /// <summary>
        /// Occurs when dialog is requested. The UI layer will listen for this event in order to show the correct UI element.
        /// </summary>
        public event EventHandler<DialogRequestedEventArgs> DialogRequested;

        public event EventHandler<LogEventArgs> LogEvent;

        public abstract string QuotedTableName { get; }

        public IClass1DataSource DataSource { get; }

        public FixItOperationCollection FixItOperations => GetNew<FixItOperationCollection>();

        public abstract string FullName { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is working on one or more jobs. 
        /// </summary>
        /// <value><c>false</c> if this instance is working on something; otherwise, <c>true</c>.</value>
        public bool Idle { get => m_WorkCount == 0; }

        public int? IndexCount { get => Get<int?>(); set => Set(value); }

        /// <summary>
        /// Gets or sets the maximum size of the sample used when analyzing the table.
        /// </summary>
        /// <value>The maximum size of the sample.</value>
        public long MaxSampleSize { get => GetDefault<long>(10000); set => Set(value); }

        public long? RowCount { get => Get<long?>(); set => Set(value); }

        public int SortIndex { get => Get<int>(); set => Set(value); }

        //Task-1: Replace with a global error logger.
        public string Status { get { return Get<string>(); } set { Set(value); } }

        /// <summary>
        /// Gets a value indicating whether the underlying DataSource supports index analysis.
        /// </summary>
        /// <value><c>true</c> if [supports indexes]; otherwise, <c>false</c>.</value>
        public virtual bool SupportsIndexes { get => false; }

        public virtual bool SupportsSparse { get => false; }

        /// <summary>
        /// Requests the dialog to be displayed.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public void RequestDialog(object dataContext)
        {
            DialogRequested?.Invoke(this, new DialogRequestedEventArgs(dataContext));
        }

        /// <summary>
        /// Call to indicate that a job is in process.
        /// </summary>
        protected void StartWork()
        {
            m_WorkCount += 1;
            OnPropertyChanged("Idle");
        }


        /// <summary>
        /// Call to indicate that a job has been completed, successfully or otherwise.
        /// </summary>
        protected void StopWork()
        {
            m_WorkCount -= 1;
            OnPropertyChanged("Idle");
        }

        public bool? IsHeap
        {
            get => GetDefault<bool?>(null);
            set
            {
                if (Set(value))
                    UpdateSuggestions();
            }
        }

        protected abstract void UpdateSuggestions();
    }
}


