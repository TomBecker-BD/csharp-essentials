using System;
namespace Essentials
{
    public class MessageBoxErrorHandler : IErrorHandler
    {
        ILogger _logger;
        IMessageBox _messageBox;

        public MessageBoxErrorHandler(ILogger logger, IMessageBox messageBox)
        {
            _logger = logger;
            _messageBox = messageBox;
        }

        public void HandleError(string operation, Exception ex)
        {
            string message = string.Format("Could not {0} because {1}", operation, ex.Message);
            _logger.Error(ex, message);
            _messageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
