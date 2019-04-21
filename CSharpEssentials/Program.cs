using System;

namespace Essentials
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            try
            {
                // Run the application
                Console.WriteLine("Hello World!");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error starting the application");
#if GUI
                // In a GUI application, display an error dialog
                MessageBox.Show(string.Format("Error starting the application: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                LogError(ex, "Unhandled exception");
            }
        }

        static void LogError(Exception ex, string message)
        {
            // Change to use your real application logger
            Console.Error.WriteLine("{0}: {1}", message, ex.ToString());
        }
    }
}
