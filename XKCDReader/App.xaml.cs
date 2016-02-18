using System.Windows;
using XKCDReader.Services;

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
				DataContext = new MainWindowViewModel(new InteractionService(), new ComicService(), new PropertiesService())
			};
			MainWindow.Show();
		}
	}
}
