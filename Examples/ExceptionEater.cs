using System;
namespace Essentials.Examples
{
    public class ExceptionEater
    {
        public void BadFunction()
        {
            try
            {
                DoSomething();
            }
            catch (Exception ex)
            {
                if (ex != null) { }
            }
        }

        void DoSomething()
        {
        }
    }
}
