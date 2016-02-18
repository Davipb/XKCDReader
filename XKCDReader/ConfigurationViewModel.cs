using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using XKCDReader.Services;

namespace XKCDReader
{
	public class ConfigurationViewModel : INotifyPropertyChanged
	{
		public IInteractionService Interaction { get; }
		public IComicService ComicManager { get; }
		public IPropertiesService Properties { get; }

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

		public ConfigurationViewModel(IInteractionService message, IComicService comicManager, IPropertiesService properties)
		{
			Interaction = message;
			ComicManager = comicManager;
			Properties = properties;

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
			LoadCurrent = Properties.LoadCurrent;
			SaveCache = Properties.SaveCache;
			PurgeComic = Properties.PurgeComic;
		}

		/// <summary>
		/// Saves this instance's properties to the application settings
		/// </summary>
		void SaveToProperties()
		{
			Properties.LoadCurrent = LoadCurrent;
			Properties.SaveCache = SaveCache;
			Properties.PurgeComic = PurgeComic;
			Properties.Save();
		}

		/// <summary>
		/// Resets the application's settings to the default and loads them to this instance's properties
		/// </summary>
		void Reset()
		{
			Properties.Reset();
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
				Interaction.ShowMessage($"Cache successfully cleared.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (System.IO.IOException e)
			{
				Interaction.ShowMessage($"Error deleting cache: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
