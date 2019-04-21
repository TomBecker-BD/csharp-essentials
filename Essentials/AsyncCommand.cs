using System;
using System.Threading.Tasks;

namespace Essentials
{
    public class AsyncCommand : IAsyncCommand
    {
        string _operation;
        IErrorHandler _errorHandler;
        Predicate<object> _canExecute;
        Func<object, Task> _execute;
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

        public AsyncCommand(string operation, IErrorHandler errorHandler, Predicate<object> canExecute, Func<object, Task> execute)
        {
            _operation = operation;
            _errorHandler = errorHandler;
            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return (!Executing) && _canExecute(parameter);
        }

        public Task ExecuteAsync(object parameter)
        {
            Executing = true;
            return _execute(parameter)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        _errorHandler.HandleError(_operation, t.Exception);
                    }
                    Executing = false;
                });
        }

        public void Execute(object parameter)
        {
            ExecuteAsync(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
