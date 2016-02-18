using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XKCDReader.Services;

namespace XKCDReader
{
	public class AddComicViewModel : INotifyPropertyChanged
	{
		public IInteractionService Interaction { get; }
		public IComicService ComicManager { get; }

		int comicNumber;
		public int ComicNumber
		{
			get { return comicNumber; }
			set
			{
				if (value < 0) return;

				comicNumber = value;
				OnPropertyChanged();
			}
		}

		bool downloading = false;
		public bool Downloading
		{
			get { return downloading; }
			protected set { downloading = value; OnPropertyChanged(); }
		}

		public AsyncRelayCommand DownloadCommand { get; }
		public XKCDComic Result { get; private set; }
		public Predicate<int> ComicValid { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;


		public AddComicViewModel(IInteractionService message, IComicService comicManager)
		{
			Interaction = message;
			ComicManager = comicManager;

			DownloadCommand = new AsyncRelayCommand(Download, (o) => !Downloading && ComicValid(ComicNumber));

			PropertyChanged += (o, e) =>
			{
				if (e.PropertyName == "Downloading" || e.PropertyName == "ComicNumber")
					DownloadCommand.RaiseCanExecuteChanged();
			};
		}

		/// <summary>
		/// Raises the PropertyChanged event
		/// </summary>
		/// <param name="name">The name of the property that was changed. Leave to default to infer automatically.</param>
		protected void OnPropertyChanged([CallerMemberName]string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		/// <summary>
		/// Tries to download the comic with number ComicNumber, then closes the window
		/// </summary>
		/// <param name="param">Command parameter, expected to be the current Window</param>
		/// <returns>A Task representing the ongoing method</returns>
		async Task Download(object param)
		{
			var window = param as Window;

			var previousCursor = Interaction.MouseOverride;
			Interaction.MouseOverride = Cursors.Wait;

			Downloading = true;

			try
			{
				Result = await ComicManager.GetWithNumber(comicNumber);
				window.DialogResult = true;
			}
			catch (Exception e) when (e is WebException || e is Newtonsoft.Json.JsonException)
			{
				Interaction.ShowMessage($"Error downloading comic info:\n{e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				window.DialogResult = false;
			}

			Downloading = false;

			Interaction.MouseOverride = previousCursor;
			window.Close();
		}
	}
}
