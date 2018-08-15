using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Tortuga.Chain;
using Tortuga.Drydock.Models.Access;
using Tortuga.Drydock.Models.PostgreSql;
using Tortuga.Drydock.Models.SQLite;
using Tortuga.Drydock.Models.SqlServer;
using Tortuga.Sails;

namespace Tortuga.Drydock.Models
{
    /// <summary>
    /// This represents a database connection and any cached data about that database.
    /// </summary>
    /// <seealso cref="Tortuga.Anchor.Modeling.ModelBase" />
    public abstract class DatabaseVM : ViewModelBase
    {
        //public abstract IDatabaseMetadataCache DatabaseMetadata { get; }

        public IClass1DataSource DataSource { get => GetDataSource(); }

        public static DatabaseVM Create(DatabaseType type, string connectionString)
        {
            switch (type)
            {
                case DatabaseType.SqlServer:
                    return new SqlServerDatabase(new SqlServerDataSource(connectionString));

                case DatabaseType.PostgreSql:
                    return new PostgreSqlDatabase(new PostgreSqlDataSource(connectionString));

                case DatabaseType.Access:
                    return new AccessDatabase(new AccessDataSource(connectionString));

                case DatabaseType.SQLite:
                    return new SQLiteDatabase(new SQLiteDataSource(connectionString));

                //case DatabaseType.MySQL:
                //    return new MySQLDatabase(new SQLiteDataSource(connectionString));

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        public abstract Task LoadSchemaAsync();

        protected abstract IClass1DataSource GetDataSource();

        public ICommand PreliminaryAnalysisCommand => GetCommand(async () => await PreliminaryAnalysisAsync());

        public abstract Task PreliminaryAnalysisAsync();

        public abstract Task PreliminaryAnalysisAsync(TableVM table);

        private int m_WorkCount;

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

        /// <summary>
        /// Attaches the UI events to the view models.
        /// </summary>
        /// <param name="dialogRequestedEventHandler">The dialog requested event handler.</param>
        public abstract void AttachUIEvents(
            EventHandler<DialogRequestedEventArgs> dialogRequestedEventHandler,
            EventHandler<LogEventArgs> logEventHandler
            );
    }
}