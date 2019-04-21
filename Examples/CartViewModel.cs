using System.Collections.Generic;
using System.Threading.Tasks;

namespace Essentials.Examples
{
    public class CartViewModel
    {
        AsyncCommand _checkout;
        List<string> _cart = new List<string>();

        public IAsyncCommand CheckoutCommand
        {
            get { return _checkout; }
        }

        public List<string> Cart
        {
            get { return _cart; }
            set
            {
                _cart = value;
                _checkout.RaiseCanExecuteChanged();
            }
        }

        public CartViewModel(IErrorHandler errorHandler)
        {
            _checkout = new AsyncCommand("checkout", errorHandler, CanCheckout, Checkout);
        }

        bool CanCheckout(object parameter)
        {
            return _cart.Count > 0;
        }

        Task Checkout(object parameter)
        {
            return Task.Run(() =>
            {
                // TODO: Confirm order and payment
                Cart = new List<string>();
            });
        }
    }
}
