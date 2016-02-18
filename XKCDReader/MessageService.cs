using System.Windows;

namespace XKCDReader
{
	public interface IMessageService
	{
		MessageBoxResult Show(string text, string caption, MessageBoxButton button, MessageBoxImage icon);
	}

	public class MessageService : IMessageService
	{
		public MessageBoxResult Show(string text, string caption, MessageBoxButton button, MessageBoxImage icon)
			=> MessageBox.Show(text, caption, button, icon);
	}
}
