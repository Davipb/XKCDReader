using System.Windows;
using System.Windows.Input;

namespace XKCDReader.Services
{
	public interface IInteractionService
	{
		Cursor MouseOverride { get; set; }
		MessageBoxResult ShowMessage(string text, string caption, MessageBoxButton button, MessageBoxImage icon);
	}

	public class InteractionService : IInteractionService
	{
		public Cursor MouseOverride
		{
			get { return Mouse.OverrideCursor; }
			set { Mouse.OverrideCursor = value; }
		}

		public MessageBoxResult ShowMessage(string text, string caption, MessageBoxButton button, MessageBoxImage icon)
			=> MessageBox.Show(text, caption, button, icon);
	}
}
