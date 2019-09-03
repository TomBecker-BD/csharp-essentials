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

Where it gets sticky is event handlers. If an object A listens for events from 
object B, object B has a hidden reference to object A so B’s event can call A‘s 
event handler. Forget to remove A’s event handler, and the garbage collector 
thinks A is still in-use. Not only will A’s memory not be reclaimed, A will 
continue to receive events from B. Object A becomes a zombie object. If you have 
a bug where an event handler is called more times than expected, look for 
objects that you expected to be deleted but are still receiving events. 

Memory is just one kind of resource. If an object has files open, or a network 
connection, or is using a thread, or a mutex, when you are done with the object, 
it needs to release all its open resources. Garbage collection is lazy — it is 
more efficient to reclaim memory only when the application needs more memory. 
But the lazy approach does not work well for other kinds of resources. You can 
have the object release resources when it is finalized by the garbage collector. 
That is not wrong — it’s good to do just in case. But it means that resources 
tend to be released at the worst possible time. For example, suppose an 
application uses several temporary files. When it is done with a file, it 
removes its reference to the object responsible for the file. When the garbage 
collector finalizes the object, the object closes the file. But this means the 
application does not close any of its temporary files until it gets low on 
memory, so it tries to close all of them at the same time, which causes it to 
become unresponsive. A better approach is to release non-memory resources as 
soon as possible. The application runs more smoothly, and it also uses fewer 
operating system resources. 

Fortunately .NET has the Disposable pattern. If an object listens for events, 
or if it uses non-memory resources, implement IDisposable. In the Dispose 
method have it remove all its event handlers and close, dispose or otherwise 
release all its non-memory resources. It should also call Dispose on any other 
objects that it is responsible for. Every Disposable object must have someone 
responsible for calling its Dispose method. This leads to a hierarchy of 
objects, where every object is Disposable, because even if it doesn’t have 
event handlers or non-memory resources, it might have child objects that do. 

Some programmers will say that implementing the Disposable pattern on every 
object is crazy and you should use weak references. A weak reference is like 
a regular reference, except it does not prevent garbage collection of the 
referenced memory. Before following a weak reference the code has to check that 
it is still valid. There even is a weak event mechanism that uses a weak 
reference for the target object. Weak references are a good thing. .NET should 
have used weak references in its implementation of events. Unfortunately, it 
used strong references. Any .NET application if it is non-trivial will have 
several event handlers for .NET events that use regular references. Even if it 
uses weak references as much as possible, it still needs to implement the 
Disposable pattern. Also, the application will still need to manage non-memory 
resources. Weak references help only with memory. In practice I find it easier 
to use regular references.

Implementing the Disposable pattern is important for a large application that 
runs over a long time, where objects have different lifetimes, and resources 
are opened and need to be released. If an application is simple, runs for a 
short time, and does just one thing, its memory and resources will be released 
when the application exits. .NET is great for programming trivial applications. 
But it also is used frequently for programming very large non-trivial 
applications. In that case, design the object hierarchy and implement the 
Disposable boilerplate on all the objects You will be glad you did.

Next: Properties
