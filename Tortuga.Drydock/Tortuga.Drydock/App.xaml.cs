using Microsoft.Extensions.Logging;
using System.Windows;
using Tortuga.Drydock.ViewModels;
using Tortuga.Sails;

namespace Tortuga.Drydock;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	static ILogger s_Log = null!;

	protected override void OnStartup(StartupEventArgs e)
	{
		var loggerFactory = LoggerFactory.Create(
			builder => builder
				.AddConsole()
				.AddDebug()
			);

		s_Log = loggerFactory.CreateLogger<App>();

		//XmlConfigurator.Configure();

		DispatcherUnhandledException += Current_DispatcherUnhandledException;
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		ViewModelBase.UnhandledCommandError += ViewModelBaseImproved_UnhandledCommandEvent;

		base.OnStartup(e);

		s_Log.LogInformation("Starting Tortuga Drydock");

		MainWindow = new MainWindow()
		{
			DataContext = new MainViewModel(loggerFactory)
		};
		MainWindow.Show();
	}

	void ViewModelBaseImproved_UnhandledCommandEvent(object? sender, UnhandledViewModelExceptionEventArgs e)
	{
		s_Log.LogError(e.Exception, "Unhandled ViewModel Exception");
		e.Handled = true;
	}

	void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
	{
		s_Log.LogCritical(e.Exception, "Unhandled Dispatcher Exception");
		e.Handled = true;
	}

	void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		s_Log.LogCritical((Exception)e.ExceptionObject, "Unhandled AppDomain Exception");
	}
}