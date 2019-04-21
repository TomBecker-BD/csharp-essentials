using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Essentials
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}
