using System;
using System.Windows.Input;

namespace Essentials
{
    public class DelegateCommand : ICommand
    {
        string _operation;
        IErrorHandler _errorHandler;
        Predicate<object> _canExecute;
        Action<object> _execute;
        bool _executing;

        public event EventHandler CanExecuteChanged;

        public bool Executing
        {
            get { return _executing; }
            private set
            {
                if (_executing != value)
                {
                    _executing = value;
                    RaiseCanExecuteChanged();
                }
            }
        }

        public DelegateCommand(string operation, IErrorHandler errorHandler, Predicate<object> canExecute, Action<object> execute)
        {
            _operation = operation;
            _errorHandler = errorHandler;
            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            Executing = true;
            try
            {
                _execute(parameter);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(_operation, ex);
            }
            finally
            {
                Executing = false;
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
