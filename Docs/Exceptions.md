# Exception Safety

You’re doing things right and you have a well-written C# .NET program. It 
implements the IDisposable pattern for reliable resource management. It has 
top-level error handling, so when there is an exception it won’t crash. 
But what if an exception leaves objects in a partially modified state? There 
are so many places where an exception can be thrown. How can you handle all the 
possible cleanup scenarios? How can you ensure the program will operate 
correctly and reliably after an exception? 

If you program in C++ you may already know the answer! The C++ Standard Library 
is exception safe, and there are excellent white papers and tutorials on 
exception safety. (See www.boost.org/community/exception_safety.html and 
exceptionsafecode.com). The same design patterns work well in C# with only minor 
changes for syntax. 

## Goal

After there is an exception executing a command, the program should be in the 
same state as before it tried to execute the command. 

Note that this is a limited goal. It applies only to programs that execute 
various commands and need to keep running. For example an interactive GUI 
application or a server. Only the top-level commands in the program need to 
fully recover from exceptions. 

## Strategy

Exception safety is a contract. When a method is called, it guarantees a certain 
level of exception safety. Not every method has to guarantee the highest level 
of safety. You just need to know what level of safety it guarantees. If a method 
provides at least basic exception safety, you can always add higher level code 
to guarantee whatever level of safety you need. Here are the levels: 

* *Basic* exception safety: If a method has an exception, affected objects can 
  be disposed safely, and no resources are leaked. 
* *Strong* exception safety: If a method has an exception, the state of the 
  program is left the same as it was before the method was called. 
* *No-throw* exception safety: The method always succeeds and never emits an 
  exception. 

Let’s look at how each level of exception safety can be implemented, and how to 
put it all together. 

## Basic Exception Safety

Basic exception safety is the minimum requirement. Basic exception safety is 
good enough for most methods, and it is not difficult. Consider the following 
example: 

```csharp
    public void ConnectAmp()
    {
        _amp?.Dispose();
        _amp = new Amp();
        _amp.Level = 11;
    }
    
    public void Dispose()
    {
        _amp?.Dispose();
        _effect?.Dispose();
    }
```

Assuming the “Amp” class implementation is basic exception safe, this code is 
also basic exception safe. 

* If the Amp constructor throws an exception, the Amp object is not created, so 
  there is no resource leak. The Dispose method is safe to call. 
* If setting the Level throws an exception, it is okay for basic exception 
  safety, because the Amp is not leaked and the Dispose method is safe to call. 

Let’s consider a method that needs work: 

```csharp
    public void Config(string path) // BAD
    {
        var reader = File.OpenText(path);
        _effect = new Effect(reader.ReadLine());
        _amp = new Amp()
        {
            Level = int.Parse(reader.ReadLine())
        };
        reader.Close();
    }
```

If there is an exception after opening the file, the StreamReader is leaked. 
That can be fixed by putting it in a ‘using’ block. 

If setting the Level throws an exception, the new Amp is leaked, because the 
exception occurs before it is been assigned to ‘_amp’. 

But wait, there’s more. If the object already had an amp, the old amp is leaked 
when it is replaced by the new one. Same for the effect object. The method is 
not safe to call, even if it completes without any exceptions. 

Here is a better version that is basic exception safe: 

```csharp
    public void Config(string path) // BASIC
    {
        _amp?.Dispose();
        _effect?.Dispose();
        using (var reader = File.OpenText(path))
        {
            _amp = new Amp();
            _amp.Level = int.Parse(reader.ReadLine());
            _effect = new Effect(reader.ReadLine());
        }
    }
```

As you can see, it is not hard to achieve basic exception safety. 

There is a special case for constructors. If an exception occurs in a 
constructor, there is no way for the caller to dispose the failed object. 
The .NET runtime will delete the memory that was allocated for the failed 
object, but any resources the constructor allocated may be leaked. Here is an 
example constructor that needs work: 

```csharp
    public Guitar(string effectName, int level) // BAD
    {
        _effect = new Effect(effectName);
        _amp = new Amp() { Level = level };
    }
```

And here is a better version that is basic exception safe: 

```csharp
    public Guitar(int level, string effectName) // BASIC
    {
        try
        {
            _amp = new Amp();
            _amp.Level = level;
            _effect = new Effect(effectName);
        }
        catch
        {
            _amp?.Dispose();
            _effect?.Dispose();
            throw;
        }
    }
```

If another developer asks (as they do) “Why not call the Guitar class Dispose 
method and save some code?”, the answer is simple: Dispose may be virtual, and 
virtual methods should never be called from constructors.

C# allows class fields to be initialized where they are declared. This is great 
for primitive types that do not need to be disposed. Objects that need to be 
disposed should be created in a constructor. 

## Strong Exception Safety

It takes a bit of extra work to implement strong exception safety, but you only 
need to do it for a few high-level methods that are used as commands. 

Here is an example of the ConnectAmp method with strong exception safety added: 

```csharp
    public void ConnectAmp() // STRONG
    {
        Amp tempAmp = new Amp();
        try
        {
            tempAmp.Level = 11;
        }
        catch
        {
            tempAmp.Dispose();
            throw;
        }
        _amp?.Dispose();
        _amp = tempAmp;
    }
```

If the new Amp constructor throws, the object is left unmodified. If the set 
Level property throws, the temp amp is disposed so there is no leak, and the 
object is left unmodified. 

In the success case, the old amp, if there is one, is disposed, and the new amp 
is assigned. Dispose methods are supposed to never throw, and assignment will 
never throw, so the last two lines do not need exception handling. 

## No-Throw Exception Safety

Most methods do not need to provide no-throw exception safety. But there is one 
category of no-throw exception safe methods that are very useful. They are also 
very easy to implement. That category is swap methods. Here is the simplest swap 
method: 

```csharp
    public static void Swap<T>(ref T a, ref T b) // NOTHROW
    {
        T temp = a;
        a = b;
        b = temp;
    }
```

Swap uses only assignment statements. Assignment statements always succeed and 
never throw. Therefore swap always succeeds and never throws. 

You can build up swap methods that swap more complex objects in place. For 
example: 

```csharp
    public static void Swap(Guitar a, Guitar b) // NOTHROW
    {
        Util.Swap(ref a._amp, ref b._amp);
        Util.Swap(ref a._effect, ref b._effect);
    }
```

This would allow players to swap guitars without allocating a new guitar. 

## Exception Neutral Coding Pattern

A try/catch block has two code paths. In the success case, the catch block is 
skipped, in the error case it is executed. Exceptions happen rarely, so error 
cleanup code does not get tested as much and is more likely to have bugs.

Exception neutral code has a single code path. The same cleanup code for the 
success case is also used for cleanup after exceptions. The cleanup code is 
easier to test and less likely to have bugs. 

Consider a strong exception safe method that allocates a new disposable object 
in a temporary variable. If it succeeds, the temp object is kept and the old 
object that it replaces is disposed. If there is an exception, the temp object 
is disposed and the old object is kept. 

```csharp
    temp = new Thing();
    try
    {
        temp.Initialize();
        current?.Dispose();
        current = temp;
    }
    catch (Exception ex)
    {
        temp?.Dispose();
    }
```

In the success case the old object is disposed and replaced with the new 
initialized temp object. In the error case, the temp object is disposed so 
there isn’t a leak. 

If we use swap, the method becomes simpler. It is still strong exception safe. 
And it is easier to test, because the cleanup code is the same for all cases: 

```csharp
    try
    {
        temp = new Thing();
        temp.Initialize();
        Swap(ref current, ref temp);
    }
    finally
    {
        temp?.Dispose();
    }
```

In the success case, the old object is swapped with the temp object. When the 
temp object is disposed, it actually is pointing to the old object, so the old 
object is disposed. In the error case, the swap method isn’t called, and the 
temp object is disposed. 

The other part of the exception neutral coding pattern is a strict ordering of 
statements based on whether they can throw and whether they allocate resources 
that require disposing. 

```csharp
    // Code that may throw exceptions but doesn't require cleanup. 
    try
    {
        // Code that may throw exceptions and requires cleanup. 
        // No-throw exception safe code (such as Swap).
    }
    finally
    {
        // Cleanup code.
    }
```

Here is an exception neutral version of the ConnectAmp method: 

```csharp
    public void ConnectAmp() // STRONG
    {
        Amp tempAmp = new Amp();
        try
        {
            tempAmp.Level = 11;
            Util.Swap(ref _amp, ref tempAmp);
        }
        finally
        {
            tempAmp.Dispose();
        }
    }
```

If setting the Level property throws an exception, the finally block disposes 
‘tempAmp’. 

In the success case, ‘_amp’ and ‘tempAmp’ are swapped and the finally block 
disposes the old amp that used to be in ‘_amp’. 

As you can see, the exception neutral code is shorter and simpler. It has less 
computational complexity so it is easier to test and debug. It still has strong 
exception safety. 

## Minor C# Gotchas

Generally ‘using’ blocks are simpler and easier to use than ‘try/finally’. 
However, exception neutral code needs to swap the ‘using variable’ with another 
value. The C# compiler does not allow setting a ‘using variable’. 

But what about smart pointers? In C++ the exception neutral coding pattern is 
even shorter and simpler because it can use smart pointers. Unfortunately, 
smart pointers are not an option in C#. Feel free to ask if you’d like the gory 
details. The bottom line is that you can write high quality code in C#, but you 
have to write a lot of boilerplate. 

## Conclusion

Exception safe code is a set of design patterns. The C# compiler doesn’t tell 
you if a method is exception safe or what level of safety it provides. You have 
to read the code. As you get more attuned to exception safe coding, you will 
be able to quickly spot unsafe code and you will wonder why it wasn’t written 
to be exception safe in the first place, because it would take just a few 
simple tweaks. Also, you will find yourself delighting in writing strong 
exception safe code that is exception neutral and easily tested. It feels 
really good to know that your code is unbreakable. Enjoy!
