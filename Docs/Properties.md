# Properties

Microsoft keeps making it easier to implement properties in C#. The art is in 
knowing which design pattern to use in each situation. Sometimes it makes sense 
to use the shiny new syntax, sometimes it is better to go old school. 

This article focuses on a few recommended design patterns that you can use.

## State Objects

The simplest class with properties is an immutable state object. This is a great 
design pattern. It keeps the state data separate from the logic. It is 
thread-safe. The state is updated by replacing the object. This design pattern 
is common with modern state-based web frameworks. 

An easy way to implement an immutable state object is using auto properties with 
private set methods, and initialize them in the class constructor. 

```csharp
public class AmpState
{
    public int Level { get; private set; }

    public AmpState(int level)
    {
        Level = level;
    }
}
```

## Binding

But what if you are implementing a view model? Auto properties don’t support 
binding. This is where have to go old school. 

```csharp
public class AmpViewModel : INotifyPropertyChanged
{
    int _level;

    public int Level
    {
        get { return _level; }
        set
        {
            if (_level != value)
            {
                _level = value;
                OnPropertyChanged(nameof(Level));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, 
            new PropertyChangedEventArgs(propertyName));
    }
}
```

The key elements are: 

* The class implements INotifyPropertyChanged.
* There is a backing field for the property.
* The set method short-circuits if the value hasn’t changed.
* The nameof keyword makes it easy to pass the property name.

The short-circuit code is essential to prevent unwanted PropertyChanged events. 
The new value is checked against the backing field. The property value is 
changed only if it is different. 

The property could be implemented using the lambda operator (=>) but you still 
need to implement a backing field and essentially the same code. 

There are many projects online for semi-automatically implementing binding 
support. As far as I can tell they are not ready for prime time. 

## Managing Event Handlers

A property can help modularize the code for adding and removing event handlers. 
For example, suppose a class needs to receive PropertyChanged events from an 
IAmp: 

```csharp
    IAmp _amp;

    IAmp Amp
    {
        get { return _amp; }
        set
        {
            if (_amp != value)
            {
                if (_amp != null)
                {
                    _amp.PropertyChanged -= Amp_PropertyChanged;
                }
                _amp = value;
                if (_amp != null)
                {
                    _amp.PropertyChanged += Amp_PropertyChanged;
                }
            }
        }
    }
```

The property ensures that the Amp_PropertyChanged event handler is removed from 
the old amp and added to the new amp. When the class is disposed, it needs to 
remove its event handlers. Using a property makes it very easy: 

```csharp
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Amp = null;
        }
    }
```

Notice that the example Amp property is private. This design pattern works fine 
with private properties. It also works if the class needs to receive multiple 
events from the same source. However, if different events need to be added and 
removed at different times, a single property is not going to work. It could be 
done with multiple properties if each has its own backing field. In that case 
it might be simpler to not use a property. 

## Fields

A quick web search will find a lot of advice to use properties instead of public 
fields. But properties aren’t magic. A publicly settable property is semantically 
equivalent to a public field. Both are thread-unsafe, and both add coupling that 
causes computational complexity you don’t want in your design. 

```csharp
    public int Level;

    public int Level { get; set; }
```

Maybe there is a good reason for creating a mutable data structure where data 
elements are settable. In that case, just use fields. A field is a memory 
location. A property is a memory location plus two functions. 

You can also use fields for immutable data. The following two declarations are 
semantically equivalent: 

```csharp
    public readonly int Level;

    public int Level { get; private set; }
```

If you don’t need the specific features of a property, prefer a field because it
is simpler. 

If you need to use properties, make them immutable if you can. They will be more 
reliable and perform better. 

## Using Properties Safely

Consider the following code: 

```csharp
        const int MaxLevel = 11;
        if (guitar.Amp.Level < MaxLevel)
        {
            guitar.Amp.Level = MaxLevel;
        }
```

It isn’t thread-safe. You might be surprised when after it runs for a while, 
your drummer explodes, but you shouldn’t be. The code gets the guitar.Amp twice. 
In multi-threaded code there is no guarantee that it will be the same Amp both 
times. (This would be true even if using fields instead of properties.) 

Also, you don’t know if the Amp property is a simple getter or more complicated. 
Or it could be simple now and it could be changed later. It is good to keep in 
mind that property get and set methods are functions and not always simple ones. 
Don’t call functions more than necessary. 

Putting an object reference in a local variable helps because the code has to 
get the object only once, instead of twice, and you can be sure it is the same 
object both times. For example: 

```csharp
        var amp = guitar.Amp;
        if (amp.Level < MaxLevel)
        {
            amp.Level = MaxLevel;
        }
```

The new code still accesses the Level property twice, but they are different 
functions, so it cannot be optimized further without refactoring. A better 
approach would be an immutable state object. 

## The Bottom Line

Properties in C# .NET are easy, but doing it correctly involves writing a lot of 
boilerplate. The good news is that writing boilerplate is easy. You just need to 
know which design pattern to choose in each situation.

Next: [Error handling](ErrorHandling.md)
