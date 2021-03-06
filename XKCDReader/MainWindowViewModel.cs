﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XKCDReader.Services;

namespace XKCDReader
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		const string ConfigurationFile = "config";

		public ObservableCollection<XKCDComic> Comics { get; } = new ObservableCollection<XKCDComic>();

		public IInteractionService Interaction { get; }
		public IComicService ComicManager { get; }
		public IPropertiesService Properties { get; }

		XKCDComic selectedComic = null;
		public XKCDComic SelectedComic
		{
			get { return selectedComic; }
			set { selectedComic = value; OnPropertyChanged(); }
		}

		bool downloading = false;
		public bool Downloading
		{
			get { return downloading; }
			set { downloading = value; OnPropertyChanged(); }
		}

		public RelayCommand AddComicCommand { get; }
		public AsyncRelayCommand AddRandomComicCommand { get; }
		public AsyncRelayCommand AddCurrentComicCommand { get; }
		public RelayCommand RemoveComicCommand { get; }
		public RelayCommand ClearComicsCommand { get; }

		public AsyncRelayCommand LoadedCommand { get; }
		public AsyncRelayCommand ClosedCommand { get; }

		public RelayCommand AboutCommand { get; }
		public RelayCommand ConfigurationCommand { get; }

		public RelayCommand CopyLinkCommand { get; }
		public RelayCommand DeleteCacheCommand { get; }
		public RelayCommand OpenLinkCommand { get; }

		public event PropertyChangedEventHandler PropertyChanged;

		int currentComicNumber = 0;

		public MainWindowViewModel(IInteractionService message, IComicService comicManager, IPropertiesService properties)
		{
			Interaction = message;
			ComicManager = comicManager;
			Properties = properties;

			AddComicCommand = new RelayCommand(AddComic, (o) => !Downloading);
			AddRandomComicCommand = new AsyncRelayCommand(AddRandomComic);
			AddCurrentComicCommand = new AsyncRelayCommand(AddCurrentComic, (o) => !Downloading);
			RemoveComicCommand = new RelayCommand(RemoveComic, () => SelectedComic != null);
			ClearComicsCommand = new RelayCommand(() => Comics.Clear());

			LoadedCommand = new AsyncRelayCommand(Loaded);
			ClosedCommand = new AsyncRelayCommand(Closed);

			AboutCommand = new RelayCommand(About);
			ConfigurationCommand = new RelayCommand(Configuration);

			CopyLinkCommand = new RelayCommand(
				(o) => Interaction.SetClipboardText(string.Format(o as string, SelectedComic.Number)),
				(o) => SelectedComic != null && o is string);

			DeleteCacheCommand = new RelayCommand(
				() => SelectedComic?.DeleteCache(),
				() => SelectedComic != null);

			OpenLinkCommand = new RelayCommand(
				(o) => Interaction.StartProcess(string.Format(o as string, SelectedComic.Number)),
				(o) => SelectedComic != null && o is string);

			PropertyChanged += (o, e) =>
			{
				if (e.PropertyName == "SelectedComic")
				{
					RemoveComicCommand.RaiseCanExecuteChanged();
					DeleteCacheCommand.RaiseCanExecuteChanged();
					OpenLinkCommand.RaiseCanExecuteChanged();
					CopyLinkCommand.RaiseCanExecuteChanged();
				}

				if (e.PropertyName == "Downloading")
				{
					AddComicCommand.RaiseCanExecuteChanged();
					AddCurrentComicCommand.RaiseCanExecuteChanged();
					AddRandomComicCommand.RaiseCanExecuteChanged();
				}
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
		/// Asks the user for a comic number, then adds the comic with that number to the list
		/// </summary>
		/// <param name="param">Command parameter, expected to be the current Window</param>
		void AddComic(object param)
		{
			var context = new AddComicViewModel(Interaction, ComicManager)
			{
				ComicNumber = SelectedComic?.Number ?? 1,
				ComicValid = (i) => i > 0 && !Comics.Any(x => x.Number == i)
			};

			var result = ComicManager.GetFromUser(context, param as Window);

			if (result != null)
			{
				Comics.Add(result);
				SelectedComic = result;
			}
		}

		/// <summary>
		/// Adds a random comic to the list. See Remarks for more info
		/// </summary>
		/// <param name="param">Command parameter, expected to be a string which can be turned into an int. See Remarks for more info.</param>
		/// <returns>A Task representing the ongoing operation</returns>
		/// <remarks>
		/// The <paramref name="param"/> parameter defines what type of random comic will be chosen:
		/// <list type="bullet">
		/// <item>
		/// <term>Type 0</term>
		/// <description>Any random comic, whether it's on the current list or not</description>
		/// </item>
		/// <item>
		/// <term>Type 1</term>
		/// <description>Only non-listed comics</description>
		/// </item>
		/// <item>
		/// <term>Type 2</term>
		/// <description>Only already-listed comics</description>
		/// </item>
		/// </list>
		/// </remarks>
		async Task AddRandomComic(object param)
		{
			var previous = Interaction.MouseOverride;
			Interaction.MouseOverride = Cursors.Wait;
			Downloading = true;

			int type;
			if (!int.TryParse(param as string, out type))
				type = 0;

			var rng = new Random();

			int random;
			if (type == 1)
			{
				// Type 1 - New (Non-Listed) Comics
				// User wants a random comic that they don't have downloaded to their list yet
				// Generate all possible comic numbers, subtract those already in the list, order them randomly then take the first one.
				// Heavy, but better than looping infinitely until we generate a valid number

				// If the list is full, this is impossible
				if (Comics.Count == currentComicNumber)
					return;

				random = await Task.Run(() =>
				Enumerable.Range(1, currentComicNumber)
				.Except(Comics.Select(x => x.Number))
				.OrderBy(x => rng.Next())
				.First());
			}
			else if (type == 2)
			{
				// Type 2 - Old (Already-Listed) Comics
				// User wants a random comic from those that they have already downloaded

				// If the list has no comics, this is impossible
				if (Comics.Count == 0)
					return;

				// Randomly select the comic number of a comic in the list
				random = await Task.Run(() =>
				Comics.Select(x => x.Number)
				.OrderBy(x => rng.Next())
				.First());
			}
			else
			{
				// Type 0/Other - Any Comic
				// User wants any comic, weather it's in their list or not
				random = rng.Next(1, currentComicNumber + 1);
			}

			var comic = Comics.FirstOrDefault(x => x.Number == random);
			if (comic == null)
			{
				comic = await ComicManager.GetWithNumber(random);
				Comics.Add(comic);
			}

			SelectedComic = comic;

			Interaction.MouseOverride = previous;
			Downloading = false;
		}

		/// <summary>
		/// Adds the current xkcd comic to the list
		/// </summary>
		/// <param name="param">Command parameter, this is ignored</param>
		/// <returns>A Task representing the ongoing operation</returns>
		async Task AddCurrentComic(object param)
		{
			Downloading = true;

			var previous = Mouse.OverrideCursor;
			Mouse.OverrideCursor = Cursors.Wait;
			var current = await ComicManager.GetWithNumber(currentComicNumber);

			if (Comics.Contains(current))
			{
				Interaction.ShowMessage($"Current comic (number {current.Number}) already loaded", "Already loaded", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			else
			{
				Comics.Add(current);
				SelectedComic = current;
			}

			Mouse.OverrideCursor = previous;
			Downloading = false;
		}

		/// <summary>
		/// Removes the currently selected comic from the list
		/// </summary>
		void RemoveComic()
		{
			var comic = SelectedComic;
			if (comic == null) return;

			Comics.Remove(comic);

			if (Properties.PurgeComic)
				comic.DeleteCache();
		}

		/// <summary>
		/// Performs startup logic for the window
		/// </summary>
		/// <param name="param">Command parameter, this is ignored</param>
		/// <returns>A Task representing the ongoing operation</returns>
		async Task Loaded(object param)
		{
			try
			{
				var currentComic = await ComicManager.GetCurrent();
				currentComicNumber = currentComic.Number;

				if (Properties.LoadCurrent)
					Comics.Add(currentComic);
			}
			catch (Exception e) when (e is System.Net.WebException || e is IOException)
			{
				Interaction.ShowMessage($"Unable to load current comic:\n{e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			var allComics = Properties.ReadConfigurationFile(ConfigurationFile);

			foreach (var comic in allComics)
			{
				try
				{
					var loaded = await ComicManager.GetWithNumber(comic);
					if (loaded != null && !Comics.Contains(loaded))
						Comics.Add(loaded);
				}
				catch (Exception e) when (e is IOException || e is System.Net.WebException)
				{
					Interaction.ShowMessage($"Unable to load comic number {comic}:\n{e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}

			SelectedComic = Comics.FirstOrDefault();
		}

		/// <summary>
		/// Performs finishing logic for the window
		/// </summary>
		/// <param name="param">Command parameter, this is ignored</param>
		/// <returns>A Task representing the ongoing operation</returns>
		async Task Closed(object param)
		{
			try
			{
				await Properties.SaveConfigurationFile(ConfigurationFile, Comics.Select(x => x.Number));
			}
			catch (IOException e)
			{
				Interaction.ShowMessage($"Error accessing configuration file:\n{e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			try
			{
				if (!Properties.SaveCache)
					ComicManager.ClearCache();
			}
			catch (IOException e)
			{
				Interaction.ShowMessage($"Error deleting cache:\n{e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>
		/// Shows the 'About' window
		/// </summary>
		/// <param name="param">Command parameter, expected to be the current Window</param>
		void About(object param)
		{
			var about = new AboutView
			{
				Owner = param as Window
			};
			about.ShowDialog();
		}

		/// <summary>
		/// Shows the configuration window
		/// </summary>
		/// <param name="param">Command parameter, expected to be the current Window</param>
		void Configuration(object param)
		{
			var config = new ConfigurationView
			{
				Owner = param as Window,
				DataContext = new ConfigurationViewModel(Interaction, ComicManager, Properties)
			};
			config.ShowDialog();
		}
	}
}
