using System.Windows;
using System.Windows.Input;
using XKCDReader.Services;

namespace XKCDReader.Tests.Services
{
	public class MockInteractionService : IInteractionService
	{
		public Cursor MouseOverride { get; set; } = Cursors.Arrow;
		public string ClipboardText { get; private set; }
		public string LastStartedProcess { get; private set; }

		public MessageBoxResult ShowMessage(string text, string caption, MessageBoxButton button, MessageBoxImage icon)
		{
			switch (button)
			{
				case MessageBoxButton.OK:
				case MessageBoxButton.OKCancel:
					return MessageBoxResult.OK;
				default:
					return MessageBoxResult.Yes;
			}
		}

		public void SetClipboardText(string text)
			=> ClipboardText = text;

		public void StartProcess(string uri)
			=> LastStartedProcess = uri;
	}
}
