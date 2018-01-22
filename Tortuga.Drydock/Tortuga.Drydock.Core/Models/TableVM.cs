using System;
using System.Windows.Input;
using Tortuga.Chain;
using Tortuga.Sails;

namespace Tortuga.Drydock.Models
{
    public abstract class TableVM : ViewModelBaseImproved
    {
        protected TableVM(IClass1DataSource dataSource)
        {
            DataSource = dataSource;
        }


        int m_WorkCount;


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

        /// <summary>
        /// Gets a value indicating whether this instance is working on one or more jobs. 
        /// </summary>
        /// <value><c>false</c> if this instance is working on something; otherwise, <c>true</c>.</value>
        public bool Idle { get => m_WorkCount == 0; }


        public virtual bool SupportsSparse { get => false; }



        /// <summary>
        /// Gets a value indicating whether the underlying DataSource supports index analysis.
        /// </summary>
        /// <value><c>true</c> if [supports indexes]; otherwise, <c>false</c>.</value>
        public virtual bool SupportsIndexes { get => false; }


        public IClass1DataSource DataSource { get; }


        /// <summary>
        /// Gets or sets the maximum size of the sample used when analyzing the table.
        /// </summary>
        /// <value>The maximum size of the sample.</value>
        public long MaxSampleSize { get => GetDefault<long>(10000); set => Set(value); }

        public long? RowCount { get => Get<long?>(); set => Set(value); }

        //Task-1: Replace with a global error logger.
        public string Status { get { return Get<string>(); } set { Set(value); } }


        public int? IndexCount { get => Get<int?>(); set => Set(value); }


        /// <summary>
        /// Occurs when dialog is requested. The UI layer will listen for this event in order to show the correct UI element.
        /// </summary>
        public event EventHandler<DialogRequestedEventArgs> DialogRequested;



        public event EventHandler<LogEventArgs> LogEvent;

        /// <summary>
        /// Requests the dialog to be displayed.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        public void RequestDialog(object dataContext)
        {
            DialogRequested?.Invoke(this, new DialogRequestedEventArgs(dataContext));
        }


        public ICommand FixNullCommand => GetCommand(FixNull);

        protected virtual void FixNull() => throw new NotSupportedException();
        protected virtual void FixAddIdentityColumn() => throw new NotSupportedException();

        public virtual bool SupportsFixNull => false;

        public virtual bool SupportsAnalyzeColumn => false;
        public virtual bool SupportsAddIdentityColumn => false;

        public bool ShowNullFixIt { get => Get<bool>(); protected set => Set(value); }

        public abstract string FullName { get; }

    }
}


