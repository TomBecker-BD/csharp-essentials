using System;
namespace Essentials
{
    public class UnitTestErrorHandler : IErrorHandler
    {
        public void HandleError(string operation, Exception ex)
        {
            Console.Error.WriteLine("Could not {0} because {1}", operation, ex.Message);
            throw new ApplicationException(string.Format("Could not {0}", operation), ex);
        }
    }
}
