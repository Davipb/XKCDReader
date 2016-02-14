using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XKCDReader
{
	public class AsyncRelayCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;

		Func<object, Task> action;
		Func<object, bool> canExecute;

		public AsyncRelayCommand(Func<object, Task> command)
		{
			action = command;
			canExecute = (o) => true;
		}

		public AsyncRelayCommand(Func<object, Task> command, Func<object, bool> can)
		{
			action = command;
			canExecute = can;
		}

		public bool CanExecute(object parameter)
			=> canExecute(parameter);

		public async void Execute(object parameter)
			=> await ExecuteAsync(parameter);

		public async Task ExecuteAsync(object parameter)
			=> await action(parameter);

		/// <summary>
		/// Raises the CanExecuteChanged event
		/// </summary>
		public void RaiseCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, new EventArgs());
		}
	}
}
