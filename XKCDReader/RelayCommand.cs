using System;
using System.Windows.Input;

namespace XKCDReader
{
	public class RelayCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;

		Action<object> action;
		Predicate<object> canExecute;

		public RelayCommand(Action command)
		{
			action = (o) => command();
			canExecute = (o) => true;
		}

		public RelayCommand(Action<object> command)
		{
			action = command;
			canExecute = (o) => true;
		}

		public RelayCommand(Action command, Func<bool> can)
		{
			action = (o) => command();
			canExecute = (o) => can();
		}

		public RelayCommand(Action<object> command, Predicate<object> can)
		{
			action = command;
			canExecute = can;
		}

		public bool CanExecute(object parameter)
			=> canExecute(parameter);

		public void Execute(object parameter)
			=> action(parameter);

		/// <summary>
		/// Raises the CanExecuteChanged event
		/// </summary>
		public void RaiseCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, new EventArgs());
		}
	}
}
