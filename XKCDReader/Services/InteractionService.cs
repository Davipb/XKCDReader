using System.Windows;
using System.Windows.Input;

namespace XKCDReader.Services
{
	public interface IInteractionService
	{
		Cursor MouseOverride { get; set; }
		MessageBoxResult ShowMessage(string text, string caption, MessageBoxButton button, MessageBoxImage icon);
		void SetClipboardText(string text);
		void StartProcess(string uri);
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

		public void SetClipboardText(string text)
			=> Clipboard.SetText(text);

		public void StartProcess(string uri)
			=> System.Diagnostics.Process.Start(uri);
	}
}
