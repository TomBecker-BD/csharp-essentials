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

Where it gets sticky is event handlers. If an object A listens for events from 
object B, object B has a hidden reference to object A so B’s event can call A‘s 
event handler. Forget to remove A’s event handler, and the garbage collector 
thinks A is still in-use. Not only will A’s memory not be reclaimed, A will 
continue to receive events from B. Object A becomes a zombie object. If you have 
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

Fortunately .NET has the Disposable pattern. If an object listens for events, 
or if it uses non-memory resources, implement IDisposable. In the Dispose 
method have it remove all its event handlers and close, dispose or otherwise 
release all its non-memory resources. It should also call Dispose on any other 
objects that it is responsible for. Every Disposable object must have someone 
responsible for calling its Dispose method. This leads to a hierarchy of 
objects, where every object is Disposable, because even if it doesn’t have 
event handlers or non-memory resources, it might have child objects that do. 

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

Some programmers will say that implementing the Disposable pattern on every object is 
crazy and you should use weak references. Unfortunately, most of the .NET APIs use strong 
references. Even if an application uses weak references as much as possible, it will end 
up with a mix of weak and strong references. It will need to use the Disposable pattern 
for the strong references. Also, it will need the Disposable pattern for releasing 
non-memory resources. 

An application's memory and resources will be released when it exits. If the application 
is simple, runs for a short time, and does just one thing, you may not need to
implement the Disposable pattern. But for a non-trivial application, the Disposable 
pattern is essential. 

Next: Properties
