using System.Windows;
using System.Windows.Input;
using XKCDReader.Services;

namespace XKCDReader.Tests
{
	public class MockMessageService : IInteractionService
	{
		public Cursor MouseOverride { get; set; } = Cursors.Arrow;

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
	}
}
