# Memory Management

The promise of C# is that it makes programming easier. It does. But not as easy 
as one might hope. There are gotchas. 

The number one gotcha in C# is memory management. C# has garbage collection. 
It would be nice if that meant we didn't have to think about memory management 
— just let the garbage collector do its thing. In practice, you need to manage 
memory as if there isn't a garbage collector — do that and the garbage collector 
works great. Don't do it and you can have memory leaks, zombie objects and severe 
performance problems. 

The garbage collector reclaims unused memory, so it can be reused. Memory is 
in-use if it is referenced by another object that is also in-use. Remove the 
last reference to an object or data structure in memory, and it becomes unused 
and the garbage collector will reclaim it. This is simple for plain-old data 
structures. Arrays, dictionaries, and hash-maps of bytes, numbers and strings 
do not require any extra work for memory management — this is a really good 
reason for using plain-old data structures where possible. 

```csharp
public class PlainOldData
{
    byte[] _data;
    Dictionary<string, int> _index;
```

Where it gets sticky is event handlers. If an object Z listens for events from 
object A, object A has a hidden reference to object Z so A’s event can call Z‘s 
event handler. Forget to remove Z’s event handler, and the garbage collector 
thinks Z is still in-use. Not only will Z’s memory not be reclaimed, Z will 
continue to receive events from A. Object Z becomes a zombie object. If you have 
a bug where an event handler is called more times than expected, look for 
objects that you expected to be deleted but are still receiving events. 

```csharp
public class ZombieObject
{
    IAmp _amp;
    public ZombieObject(IAmp amp)
    {
        _amp = amp;
        _amp.PropertyChanged += Amp_PropertyChanged;
    }
```

Resources such as open files, network connections, threads and mutexes, must be
released when they are no longer needed. If they are never released, the operating
system could run out of underlying handles, and it could crash or hang. 

You can ensure a resource is released by adding a finalizer. When the garbage collector
reclaims an object, it will call the finalizer first, if there is one. The problem with
finalizers is they don't get called until the application is running low on memory. 
This means the application holds onto resources longer than it needs to. Also, it can 
take a significant amount of time to release resources if there are a lot of them. 

```csharp
public class LazyCleanup
{
    SafeMemoryMappedFileHandle _file;

    ~LazyCleanup()
    {
        if (_file != null)
        {
            _file.Dispose();
            _file = null;
        }
    }
```

A better approach is to release resources immediately when they are no longer needed. 

## Disposable Pattern

Fortunately .NET has the Disposable pattern. If an object listens for events, 
or if it uses non-memory resources, implement IDisposable. In the Dispose 
method have it remove all its event handlers and close, dispose or otherwise 
release all its non-memory resources. It should also call Dispose on any other 
IDisposable objects that it is responsible for. 

```csharp
public class SmoothCleanup : IDisposable
{
    IAmp _amp;
    SafeMemoryMappedFileHandle _file;

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_amp != null)
            {
                _amp.PropertyChanged -= Amp_PropertyChanged;
                _amp = null;
            }
            if (_file != null)
            {
                _file.Dispose();
                _file = null;
            }
        }
    }
    public void Dispose()
    {
        Dispose(true);
    }
````

## Application Design

The easiest way to manage Disposable objects is in a tree. Each Disposable object has 
a single parent that is responsible for disposing it. If multiple objects have references
to the same Disposable object, only one of them is the parent and gets to call Dispose. 
The other objects may use the Disposable object, but they don't own it. 

Depending on the application, there may be multiple trees of Disposable objects. 

In a desktop application, each window may have a Disposable view model that is the root 
of a tree. The window's DataContext is typed as an object, and the window is really not
supposed to know the type of its view model. But it can dynamically cast the view model
to IDisposable. That way it works with any view model type, whether or not it is 
Disposable. 

```csharp
IDisposable disposableViewModel = DataContext as IDisposable;
if (disposableViewModel != null)
{
    disposableViewModel.Dispose();
}
```

If an application is document-based, a document object is the root of a tree. When a 
document is closed, calling the document's Dispose method will dispose the whole tree. 

In an ASP.NET application, controller objects are created for each request. Controllers 
do not live long and generally do not need to subscribe to events. This minimizes the 
need for disposing in controllers, but if necessary, controllers are Disposable. 
Depending on the application, other objects such as sessions may need to be the root of a
Disposable tree. 

## Alternatives and Special Cases

Some programmers may say that implementing the Disposable pattern on every object is 
crazy and you should use weak references. However, you will still need to use the 
Disposable pattern for releasing non-memory resources. Also, you will need to deal with 
.NET APIs that do not use weak events. 

An application's memory and resources will be released when it exits. If the application 
is simple, runs for a short time, and does just one thing, you may not need to
implement the Disposable pattern. However, if the code may end up being reused in a 
non-trivial application, you may want to implement the Disposable pattern anyway. 

Next: [Properties](Properties.md)
