using System;
namespace Essentials
{
    public interface IErrorHandler
    {
        void HandleError(string operation, Exception ex);
    }
}
