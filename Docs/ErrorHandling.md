# Error Handling

The .NET framework does not provide good infrastructure for handling errors. 
If an exception is unhandled, the program will crash. Considering that a big 
selling point of C# and .NET is ease of development, one would think that 
something basic like error handling would be provided in the framework. But you 
need to provide your own. 

The good news is, once you have the infrastructure in place, doing good error 
handling is easy.

## Strategy

Most of the error handling is needed for user interface commands. That is where 
most of the errors are likely to happen, and also these errors should be 
recoverable. 

The basic flow of a UI command is like this: 

1. The user initiates an operation by clicking or typing in a control. 
2. The system generates a raw event such as mouse down or key up. 
3. The system checks if the control is enabled. 
4. The control handles the raw event and generates a high level event such as 
   button clicked. 
5. An application view model handles the high level event and executes a command 
   such as checkout. 

If there is an exception executing a command, the application should display a 
user-friendly error message. The user should be able to keep using the 
application.

The best place to catch exceptions is in a common command class. If an exception 
occurs, it is most likely to occur in the process of executing the command. 
The command object can display a meaningful error message that describes exactly 
what failed. For example, “Could not checkout because of a network error.” 
The best the system or the UI control could do is display “mouse down failed” 
or “button click failed” which is not exactly helpful. 

Error handling is provided through an interface, so the appropriate 
implementation can be injected. The production code implementation displays an 
error message. The unit test implementation rethrows the exception so the test 
will fail, and so test execution isn’t blocked by an error alert. 

The application should have an unhandled exception handler for exceptions that 
occur for events other than UI commands. .NET has many kinds of user interface 
events. An exception could occur in any of them. Unhandled exceptions are not 
recoverable. You want to make sure the exception is logged and reported so the 
\exception can be prevented or handled. 

If the application uses threads, there should be a try/catch block in the 
top-level method for each thread. Otherwise the thread just terminates. 
Before using a thread, consider if it runs only for a short time, in which case 
you can use a task. If you need to use a thread for a long-running operation, 
implement the event-based asynchronous pattern. It provides a completed event 
that sends success or an exception to the UI. It can also have a progress event. 

OK, let’s dive into the implementation.

## Implementation

First let’s define the error handler interface: 

```csharp
public interface IErrorHandler
{
    void HandleError(string operation, Exception ex);
}
```

Here’s an implementation that shows a message box with an error message: 

```csharp
public class MessageBoxErrorHandler : IErrorHandler
{
    public void HandleError(string operation, Exception ex)
    {
        string message = string.Format("Could not {0} because {1}", operation, ex.Message);
        Log.Error(ex, message);
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
```

Again, it is very simple. It just needs to be factored as a service so it can 
be injected. Use whatever error logger you have chosen for your application. 

.NET provides an ICommand interface which is easy to use in WPF and XAML. 
(It can also be retro-fitted for use with WinForms.) However, there is not a 
simple ICommand implementation in .NET that does error handling. There is a 
RoutedUICommand implementation in .NET but it solves a different problem. 
There is a DelegateCommand implementation in Prism. It is a good starting point, 
but it does not handle errors. There are various implementations such as 
RelayCommand and ActionCommand available on the internet, but they do not 
handle errors. So we will provide our own.

For reference, here is the ICommand interface definition from .NET: 

```csharp
public interface ICommand
{
    event EventHandler CanExecuteChanged;
    bool CanExecute(object parameter);
    void Execute(object parameter);
}
```

The ICommand interface supports only synchronous execution. Modern .NET 
applications should be asynchronous as much as possible, so we will extend 
ICommand to support asynchronous commands. 

```csharp
public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync(object parameter);
}
```

And here is the entire AsyncCommand implementation: 

```csharp
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
```

The client using the AsyncCommand provides a string describing the command (for 
error messages), the error handler interface, and delegates for the canExecute 
and execute methods. This way AsyncCommand is completely generic and can be used 
for any command.

The WPF control will call the command’s Execute method. Execute simply calls 
ExecuteAsync. This way the command executes asynchronously without blocking the 
UI thread.

If an exception occurs executing the command, the continuation method will see 
the task is faulted and handle the error. 

The Executing property disables the command while it is executing. If you want 
to allow multiple simultaneous execution of the same command, you could leave 
that out.

Using AsyncCommand is easy. Here is an example: 

```csharp
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
```

The view model has a cart and a checkout command. The command is enabled if 
there are items in the cart. Executing the command empties the cart. The command 
is exposed as a public property so a UI control can bind to it.

Notice that the view model has full access to the AsyncCommand public API, while 
other classes only have access to the IAsyncCommand interface. 

## Unit Testing

For unit testing there is an alternate IErrorHandler implementation that throws 
an exception instead of popping up a message box. That way, the test will fail 
(as it should) and test execution will not be blocked by popup window waiting 
for you to click OK: 

```csharp
public class UnitTestErrorHandler : IErrorHandler
{
    public void HandleError(string operation, Exception ex)
    {
        Console.Error.WriteLine("Could not {0} because {1}", operation, ex.Message);
        throw new ApplicationException(string.Format("Could not {0}", operation), ex);
    }
}
```

This takes care of the normal case where an error should not occur. In the case 
where you want to test that an error is handled, use a mock IErrorHandler and 
assert that HandleError was called. 

If a unit test executes a command, it should call the ExecuteAsync method and 
Wait for the task to complete. For example: 

```csharp
[Fact]
public void TestCheckoutEmptiesCart()
{
    var vm = new CartViewModel(new UnitTestErrorHandler());
    vm.Cart = new List<string> { "Wensleydale" };
    vm.CheckoutCommand.ExecuteAsync(null).Wait();
    Assert.Empty(vm.Cart);
}
```

This gives you an easy way to make sure the command has completed before you 
try to do asserts. 

If a unit test calls the command’s Execute method, it causes a race condition. 
Don’t do that. The task may or may not finish before the assert. It might not 
finish running until after the test returns and another test is being executed. 
There will also be a race condition if the production code calls Execute 
directly. The Execute method is meant to be called only through binding from a 
UI control. For everything else, call ExecuteAsync or a lower-level function.

## Handling Unhandled Exceptions

Adding an unhandled exception handler is simple: 

```csharp
static void Main(string[] args)
{
    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    try
    {
        // Run the application
    }
    catch (Exception ex)
    {
        LogError(ex, "Error starting the application");
        MessageBox.Show(string.Format("Error starting the application: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
```

Use whatever error logger you have chosen for your application. 

The try/catch block around the main application code handles errors starting the 
application, before the main message loop is running. 

## Conclusion

In C# .NET, in order to write reliable code, you need to write a lot of 
boilerplate. Fortunately, with error handling, most of the boilerplate can be 
moved into common infrastructure. With the right infrastructure, good error 
handling is easy. Now if only the infrastructure were built into .NET.
