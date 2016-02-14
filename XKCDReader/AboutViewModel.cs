using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace XKCDReader
{
	public class AboutViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string randomCat = "";
		public string RandomCat
		{
			get { return randomCat; }
			set { randomCat = value; OnPropertyChanged(); }
		}

		Visibility catVisibility = Visibility.Collapsed;
		public Visibility CatVisibility
		{
			get { return catVisibility; }
			set { catVisibility = value; OnPropertyChanged(); }
		}

		Random rng = new Random();

		public RelayCommand CatCommand { get; }
		public RelayCommand CopyLinkCommand { get; }
		public RelayCommand OpenLinkCommand { get; }

		public AboutViewModel()
		{
			CatCommand = new RelayCommand(Cat);

			CopyLinkCommand = new RelayCommand(
				(o) => Clipboard.SetText(o as string));

			OpenLinkCommand = new RelayCommand(
				(o) => System.Diagnostics.Process.Start(o as string));
		}

		/// <summary>
		/// Toggles the easter-egg's visibility, and, if its visible, chooses a new random Cat.
		/// </summary>
		protected void Cat()
		{
			switch (CatVisibility)
			{
				case Visibility.Collapsed:
					RandomCat = $"Cats\\{rng.Next(1, 7)}.jpg";
					CatVisibility = Visibility.Visible;
					break;

				case Visibility.Visible:
					CatVisibility = Visibility.Collapsed;
					break;
			}
		}

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
