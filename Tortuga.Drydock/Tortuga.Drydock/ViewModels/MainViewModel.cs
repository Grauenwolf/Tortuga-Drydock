using log4net;
using Microsoft.Data.ConnectionUI;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tortuga.Anchor.Collections;
using Tortuga.Anchor.Modeling;
using Tortuga.Drydock.Models;
using Tortuga.Drydock.Properties;
using Tortuga.Drydock.Views;
using Tortuga.Sails;

namespace Tortuga.Drydock.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        private static readonly ILog s_Log = LogManager.GetLogger(typeof(App));

        private ObservableCollectionExtended<string> m_LogEvents = new ObservableCollectionExtended<string>();

        public ICommand AnalyzeTableCommand => GetCommand<TableVM>(async t => await AnalyzeTableAsync(t));

        [CalculatedField("DatabaseType,IsConnecting,ConnectionString")]
        public bool CanConnect => Enum.IsDefined(typeof(DatabaseType), DatabaseType) && !IsConnecting && !string.IsNullOrWhiteSpace(ConnectionString);

        [CalculatedField("DatabaseType")]
        public bool CanEditConnection
        {
            get
            {
                switch (DatabaseType)
                {
                    case DatabaseType.SqlServer:
                    case DatabaseType.Access:
                        return true;

                    default:
                        return false;
                }
            }
        }

        public ICommand ConnectCommand => GetCommand(async () => await ConnectAsync());

        public string ConnectionString
        {
            get => GetDefault(Settings.Default.LastConnectionString);
            set => Set(value);
        }

        public DatabaseVM Database { get => Get<DatabaseVM>(); set => Set(value); }
        public DatabaseType DatabaseType { get => GetDefault((DatabaseType)Settings.Default.LastDatabaseType); set => Set(value); }
        public ICommand EditConnectionCommand => GetCommand(EditConnection);

        public double Height { get => GetDefault(400); set => Set(value); }
        public bool IsConnected { get => Get<bool>(); private set => Set(value); }

        [CalculatedField("IsConnected")]
        public int IsConnectedNumber { get => IsConnected ? 1 : 0; }

        public bool IsConnecting { get => Get<bool>(); private set => Set(value); }
        public int SelectedTab { get => GetDefault(2); set => Set(value); }
        public ReadOnlyObservableCollectionExtended<string> LogEvents => m_LogEvents.ReadOnlyWrapper;
        public string Status { get => Get<string>(); private set => Set(value); }

        [CalculatedField("DatabaseType")]
        public bool UseAccess { get => DatabaseType == DatabaseType.Access; set { if (value) DatabaseType = DatabaseType.Access; } }

        [CalculatedField("DatabaseType")]
        public bool UseMySQL { get => DatabaseType == DatabaseType.MySQL; set { if (value) DatabaseType = DatabaseType.MySQL; } }

        [CalculatedField("DatabaseType")]
        public bool UsePostgreSql { get => DatabaseType == DatabaseType.PostgreSql; set { if (value) DatabaseType = DatabaseType.PostgreSql; } }

        [CalculatedField("DatabaseType")]
        public bool UseSQLite { get => DatabaseType == DatabaseType.SQLite; set { if (value) DatabaseType = DatabaseType.SQLite; } }

        [CalculatedField("DatabaseType")]
        public bool UseSqlServer { get => DatabaseType == DatabaseType.SqlServer; set { if (value) DatabaseType = DatabaseType.SqlServer; } }

        public double Width { get => GetDefault(900); set => Set(value); }

        public WindowState WindowState { get => GetDefault(WindowState.Normal); set => Set(value); }

        public async void ShowDialog(object sender, DialogRequestedEventArgs e)
        {
            switch (e.DataContext)
            {
                case FixItVM dc:
                    ShowFixItWindow(dc);
                    break;

                case DataVM dc:
                    ShowDataWindow(dc);
                    break;

                case TableVM table:
                    await AnalyzeTableAsync(table);
                    break;
            }
        }

        public void ShowFixItWindow(FixItVM dataContext)
        {
            var window = new FixItWindow() { DataContext = dataContext };
            window.Show();
        }

        public void ShowDataWindow(DataVM dataContext)
        {
            var window = new DataWindow() { DataContext = dataContext };
            window.Show();
        }

        private async Task AnalyzeTableAsync(TableVM table)
        {
            foreach (var view in Application.Current.Windows.OfType<TableWindow>())
            {
                if (view.DataContext == table)
                {
                    if (view.WindowState == WindowState.Minimized)
                        view.WindowState = WindowState.Normal;
                    view.Activate();
                    return;
                }
            }

            var window = new TableWindow() { DataContext = table };
            window.Height = Height;
            window.Width = Width;
            if (WindowState == WindowState.Maximized)
                window.WindowState = WindowState.Maximized;
            window.Show();
            if (table.RowCount == null)
                await Database.PreliminaryAnalysisAsync(table);
        }

        private async Task ConnectAsync()
        {
            if (IsConnecting)
                return;

            IsConnecting = false;
            IsConnected = false;

            try
            {
                IsConnecting = true;

                LogEvent($"Creating data source for {DatabaseType}");

                Database = DatabaseVM.Create(DatabaseType, ConnectionString);

                LogEvent($"Testing connection to {Database.DataSource.Name} ({DatabaseType})");

                await Database.DataSource.TestConnectionAsync();

                LogEvent($"Loading schema for {Database.DataSource.Name} ({DatabaseType})");

                await Database.LoadSchemaAsync();
                Database.AttachUIEvents(ShowDialog, LogEvent);

                LogEvent($"Connected to {Database.DataSource.Name}!");

                IsConnected = true;

                SelectedTab = 0;

                Settings.Default.LastConnectionString = ConnectionString;
                Settings.Default.LastDatabaseType = (int)DatabaseType;
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                LogEvent(ex.Message);
                Database = null;
                throw;
            }
            finally
            {
                IsConnecting = false;
            }
        }

        private void EditConnection()
        {
            using (var dialog = new DataConnectionDialog())
            {
                //ref: http://stackoverflow.com/questions/6895251/display-a-connectionstring-dialog/32326126#32326126

                switch (DatabaseType)
                {
                    case DatabaseType.SqlServer:
                        dialog.DataSources.Add(DataSource.SqlDataSource);
                        break;

                    case DatabaseType.Access:
                        dialog.DataSources.Add(DataSource.AccessDataSource);
                        break;
                }

                try
                {
                    dialog.ConnectionString = ConnectionString;
                }
                catch
                {
                    //Connection string format not supported.
                    dialog.ConnectionString = null;
                }

                // The way how you show the dialog is somewhat unorthodox; `dialog.ShowDialog()`
                // would throw a `NotSupportedException`. Do it this way instead:
                System.Windows.Forms.DialogResult userChoice = DataConnectionDialog.Show(dialog);

                // Return the resulting connection string if a connection was selected:
                if (userChoice == System.Windows.Forms.DialogResult.OK)
                    ConnectionString = dialog.ConnectionString;
            }
        }

        private void LogEvent(string message)
        {
            s_Log.Info(message);
            m_LogEvents.Add(message);
            Status = message;
        }

        private void LogEvent(object sender, LogEventArgs e)
        {
            LogEvent(e.Message);
        }
    }
}