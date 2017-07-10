using log4net;
using log4net.Config;
using System;
using System.Windows;
using Tortuga.Drydock.ViewModels;
using Tortuga.Sails;

namespace Tortuga.Drydock
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog s_Log = LogManager.GetLogger(typeof(App));

        protected override void OnStartup(StartupEventArgs e)
        {
            XmlConfigurator.Configure();

            DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            ViewModelBaseImproved.UnhandledCommandEvent += ViewModelBaseImproved_UnhandledCommandEvent;


            base.OnStartup(e);

            s_Log.Info("Starting Tortuga Drydock");

            MainWindow = new MainWindow()
            {
                DataContext = new MainViewModel()
            };
            MainWindow.Show();
        }

        private void ViewModelBaseImproved_UnhandledCommandEvent(object sender, UnhandledViewModelExceptionEventArgs e)
        {
            s_Log.Error("Unhandled ViewModel Exception", e.Exception);
            e.Handled = true;
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            s_Log.Fatal("Unhandled Dispatcher Exception", e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            s_Log.Fatal("Unhandled AppDomain Exception", (Exception)e.ExceptionObject);
        }
    }
}
