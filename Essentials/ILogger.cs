using System;
namespace Essentials
{
    public interface ILogger
    {
        void Error(Exception ex, string message);
    }
}
