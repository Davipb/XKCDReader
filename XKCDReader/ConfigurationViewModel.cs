using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace XKCDReader
{
	public class ConfigurationViewModel : INotifyPropertyChanged
	{
		public IMessageService Message { get; }
		public IComicService ComicManager { get; }

		bool loadCurrent;
		public bool LoadCurrent
		{
			get { return loadCurrent; }
			set { loadCurrent = value; OnPropertyChanged(); }
		}

		bool saveCache;
		public bool SaveCache
		{
			get { return saveCache; }
			set { saveCache = value; OnPropertyChanged(); }
		}

		bool purgeComic;
		public bool PurgeComic
		{
			get { return purgeComic; }
			set { purgeComic = value; OnPropertyChanged(); }
		}

		bool busy = false;
		public bool Busy
		{
			get { return busy; }
			set { busy = value; OnPropertyChanged(); }
		}

		public RelayCommand OkCommand { get; }
		public RelayCommand CancelCommand { get; }
		public RelayCommand ResetCommand { get; }
		public AsyncRelayCommand ClearCacheCommand { get; }

		public ConfigurationViewModel(IMessageService message, IComicService comicManager)
		{
			Message = message;
			ComicManager = comicManager;

			LoadFromProperties();

			OkCommand = new RelayCommand(Ok, (o) => !Busy);
			CancelCommand = new RelayCommand(Cancel, (o) => !Busy);
			ResetCommand = new RelayCommand(Reset);
			ClearCacheCommand = new AsyncRelayCommand(ClearCache, (o) => !Busy);

			PropertyChanged += (o, e) =>
			{
				if (e.PropertyName == "Busy")
				{
					OkCommand.RaiseCanExecuteChanged();
					CancelCommand.RaiseCanExecuteChanged();
					ClearCacheCommand.RaiseCanExecuteChanged();
				}
			};
		}

		/// <summary>
		/// Sets the this instance's properties to those defined in the application settings
		/// </summary>
		void LoadFromProperties()
		{
			LoadCurrent = Properties.Settings.Default.LoadCurrent;
			SaveCache = Properties.Settings.Default.SaveCache;
			PurgeComic = Properties.Settings.Default.PurgeComic;
		}

		/// <summary>
		/// Saves this instance's properties to the application settings
		/// </summary>
		void SaveToProperties()
		{
			Properties.Settings.Default.LoadCurrent = LoadCurrent;
			Properties.Settings.Default.SaveCache = SaveCache;
			Properties.Settings.Default.PurgeComic = PurgeComic;
			Properties.Settings.Default.Save();
		}

		/// <summary>
		/// Resets the application's settings to the default and loads them to this instance's properties
		/// </summary>
		void Reset()
		{
			Properties.Settings.Default.Reset();
			LoadFromProperties();
		}

		/// <summary>
		/// Saves the instance's properties to the application's settings and closes the window
		/// </summary>
		/// <param name="param">Command parameter, expected to be the current Window</param>
		void Ok(object param)
		{
			var window = param as Window;
			if (window == null) return;

			SaveToProperties();

			window.Close();
		}

		/// <summary>
		/// Closes the window without saving to the application's settings
		/// </summary>
		/// <param name="param">Command parameter, expected to be the current Window</param>
		void Cancel(object param)
		{
			var window = param as Window;
			if (window == null) return;

			window.Close();
		}

		/// <summary>
		/// Clears the cache, removing all downloaded files
		/// </summary>
		/// <param name="param">Command parameter, this is ignored</param>
		/// <returns>A Task representing the ongoing operation</returns>
		async Task ClearCache(object param)
		{
			Busy = true;

			try
			{
				await Task.Run((Action)ComicManager.ClearCache);
				Message.Show($"Cache successfully cleared.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (System.IO.IOException e)
			{
				Message.Show($"Error deleting cache: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			Busy = false;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the PropertyChanged event
		/// </summary>
		/// <param name="name">The name of the property that was changed. Leave to default to infer automatically.</param>
		protected void OnPropertyChanged([CallerMemberName]string name = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
