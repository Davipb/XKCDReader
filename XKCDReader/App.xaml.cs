using System.Windows;

namespace XKCDReader
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		void Application_Startup(object sender, StartupEventArgs args)
		{
			MainWindow = new MainWindow
			{
				DataContext = new MainWindowViewModel(new MessageService(), new ComicService())
			};
			MainWindow.Show();
		}
	}
}
