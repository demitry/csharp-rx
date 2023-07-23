<!-- TOC -->

- [Reactive extension](#reactive-extension)
    - [Course Introduction [1.]](#course-introduction-1)
    - [Overview [2.]](#overview-2)
    - [Observer Design Pattern [3.]](#observer-design-pattern-3)
        - [Notify with INotifyPropertyChanged](#notify-with-inotifypropertychanged)
        - [Use Event](#use-event)
        - [BindingList](#bindinglist)
        - [Problem](#problem)
    - [IObserver [4.]](#iobserver-4)
    - [IObservable [5.]](#iobservable-5)
    - [Quiz 1: Key Interfaces [  ]](#quiz-1-key-interfaces---)
    - [Summary [6.]](#summary-6)
    - [Overview [7.]](#overview-7)
    - [Subject [8.]](#subject-8)
        - [Get rid of an IObservable](#get-rid-of-an-iobservable)
        - [Get rid of an IObserver](#get-rid-of-an-iobserver)
    - [Unsubscribing [9.]](#unsubscribing-9)
    - [Proxy and Broadcast [10.]](#proxy-and-broadcast-10)
    - [ReplaySubject [11.]](#replaysubject-11)
    - [BehaviorSubject [12.]](#behaviorsubject-12)
    - [AsyncSubject [13.]](#asyncsubject-13)
    - [Implementing IObservable [14.]](#implementing-iobservable-14)
    - [Summary [15.]](#summary-15)
    - [Section 3 Fundamental Sequence Operators Overview [16.]](#section-3-fundamental-sequence-operators-overview-16)
    - [Simple Factory Methods [17.]](#simple-factory-methods-17)
    - [Observable.Create [18.]](#observablecreate-18)
    - [Sequence Generators [19.]](#sequence-generators-19)
    - [Converting Into Observables [20.]](#converting-into-observables-20)
    - [Sequence Filtering [21.]](#sequence-filtering-21)
    - [Sequence Inspection [22.]](#sequence-inspection-22)
    - [Sequence Transformation [23.]](#sequence-transformation-23)
    - [Sequence Aggregation [24.]](#sequence-aggregation-24)
    - [Quiz 3: Fundamental Sequence Operators [  ]](#quiz-3-fundamental-sequence-operators---)
    - [Summary [25.]](#summary-25)
    - [Overview [26.]](#overview-26)
    - [Exception Handling [27.]](#exception-handling-27)
    - [Sequence Combinators [28.]](#sequence-combinators-28)
    - [Time-Related Sequence Processing [29.]](#time-related-sequence-processing-29)
    - [Reactive Extensions Event Broker [30.]](#reactive-extensions-event-broker-30)
    - [Quiz 4: Advanced Sequence Operators [  ]](#quiz-4-advanced-sequence-operators---)
    - [Summary [31.]](#summary-31)
    - [Course Summary [32.]](#course-summary-32)
    - [Bonus Lecture: Other Courses at a Discount [33.]](#bonus-lecture-other-courses-at-a-discount-33)

<!-- /TOC -->

# Reactive extension

## Course Introduction [1.]

- Streams (stock market, sensors)
- Push model instead of Pull
- We want process this data just like static data in a list or DB
- LINQ-like ops but reactive

=> Reactive Extensions (Rx) let us do precisely this!

And more... because processing is in real time

LINQ, TPL

## Overview [2.]

- Observer Design Pattern
- IObserver < T >
- IObservable < T >

## Observer Design Pattern [3.]

### Notify with INotifyPropertyChanged

```cs
using JetBrains.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace S03ObserverDesignPattern;

public class Market : INotifyPropertyChanged
{
    private float volatility;

    public float Volatility
    {
        get => volatility;
        set
        {
            if(value.Equals(volatility)) return;
            volatility = value;
            OnPropertyChanged(nameof(Volatility));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = default)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

internal class Program
{
    static void Main(string[] args)
    {
        var market = new Market();
        market.PropertyChanged += (sender, eventArgs) =>
        {
            if (eventArgs.PropertyName == "Volatility")
            {
                Console.WriteLine("Wow! Volatility was changed");
            }
        };

        market.Volatility = 2.3F;
    }
}
```

### Use Event

```cs
namespace S03ObserverDesignPattern2;

public class Market
{
    private List<float> prices = new List<float>();

    public void AddPrice(float price)
    {
        prices.Add(price);
        // Classic:
        PriceAdded?.Invoke(sender: this, price);
    }

    // Classic:
    public event EventHandler<float> PriceAdded;
}

internal class Program
{
    static void Main(string[] args)
    {
        var market = new Market();
        
        // Subscribe:
        market.PriceAdded += (sender, f) =>
        {
            Console.WriteLine($"We got a price of {f}");
        };

        market.AddPrice(100);
    }
}
```

### BindingList

```cs
namespace S03ObserverDesignPattern3;

public class Market // observable
{
    public BindingList<float> Prices = new BindingList<float>();

    public void AddPrice(float price)
    {
        Prices.Add(price);
    }
}

internal class Program // observer
{
    static void Main(string[] args)
    {
        var market = new Market();

        market.Prices.ListChanged += (sender, eventArgs) =>
        {
            if (eventArgs.ListChangedType == ListChangedType.ItemAdded)
            {
                if (sender != null)
                {
                    float price = ((BindingList<float>)sender)[eventArgs.NewIndex];
                    Console.WriteLine($"Binding list got a price of {price}");
                }
            }
        };

        market.AddPrice(123);
    }
}
```

### Problem

But what if market crashed or no new prices added and we don't know why?

Create MarkedCrashed events?

## IObserver [4.]

```cs

internal class Program : IObserver<float>
{
    public Program()
    {
        var market = new Market();
        market.Subscribe(this);        
    }

    static void Main(string[] args)
    {
        // OnNext --> (OnError | OnCompleted) ? optional
        // Bad practice to call OnNext again. But it is possible.
    }

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        // Something went wrong in a sequence
    }

    public void OnNext(float value)
    {
        Console.WriteLine($"Market gave us new value: {value}");
    }
}
```

## IObservable [5.]

```cs
public class Market : IObservable<float>
{
    public IDisposable Subscribe(IObserver<float> observer)
    {
        throw new NotImplementedException();
    }
}
```

Subscribe method

- Subscribe() method is a glue which connects IObserver and IObservable.
- Subscribe() = Add the observer to the collection of subscriptions.
- IDisposable Subscribe
- Disposable is easy to dispose, thus ending the subscription.

## Quiz 1: Key Interfaces [  ]

## Summary [6.]

Rx expands on the Observer patters

- Supports reactive sequences
- Explicitly supports termination
  - Completion; or
  - Failure

IObservable< T > implemented by the types that generate reactive sequences

IObserver < T > generated by reactive sequence consumers

IObservable -> PUSH -> OObserver

Typically you will not implement either of this

## Overview [7.]

- Subject < T >
- Unsubscribing
- Proxy abd Broadcast
- ReplaySubject < T >
- BehaviorSubject < T >
- AsyncSubject < T >
- Implementing IObservable< T >

## Subject [8.]

IObservable and IObserver are now the part of .NET but the rest reactive stuff requires System.Reactive.

nuget System.Reactive

Subject acts like both Observer and Observable

So it can glued together with a Observer

### Get rid of an IObservable

```cs
using System.Reactive.Subjects;

namespace S07Subjects;

internal class Program : IObserver<float>
{
    public Program()
    {
        // Subject acts like both Observer and Observable
        // So it can glued together with a Observer (Program)
        
        var market = new Subject<float>();
        market.Subscribe(this);

        market.OnNext(1.23f); // Post the value

        //market.OnError(new Exception("oops"));
        
        market.OnCompleted(); // I am not calling Program.OnCompleted
        // I am calling it on a subject
        // And subject notifies the IObserver (Program)
        // about the fact that this has occurred.
        // So we got rid of an IObservable
    }

    static void Main(string[] args)
    {
        new Program();
    }

    public void OnCompleted()
    {
        Console.WriteLine("Sequence is complete");
    }

    public void OnError(Exception error)
    {
        Console.WriteLine($"We got an error: {error.Message}");
    }

    public void OnNext(float value)
    {
        Console.WriteLine($"Market gave us new value: {value}");
    }
}
```

### Get rid of an IObserver

```cs
internal class Program
{
    public Program()
    {
        var market = new Subject<float>();
        market.Subscribe(
            f => Console.WriteLine($"Price is {f}"),
            () => Console.WriteLine("Sequence is complete")
            );

        market.OnNext(1.23f);
        market.OnCompleted();
    }

    static void Main(string[] args)
    {
        new Program();
    }
}
```

## Unsubscribing [9.]

```cs
internal class Program: IObservable<float>
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }

    // return IDisposable
    // It paradigm of reactive extensions
    // Disposing a subscription is exactly the process of unsubscribing
    public IDisposable Subscribe(IObserver<float> observer)
    {
        throw new NotImplementedException();
    }
}
```

```cs
internal class Program
{
    static void Main(string[] args)
    {
        var sensor = new Subject<float>();

        using (sensor.Subscribe(Console.WriteLine))
        {
            sensor.OnNext(1);
            sensor.OnNext(2);
            sensor.OnNext(3);
        }

        sensor.OnNext(4); // We will not get the 4, just 1,2,3
        //IDisposable object used to unsubscribe from the observable sequence
        //Disposed => unsubscribed
    }
}
```

## Proxy and Broadcast [10.]

```cs
public static class ExtensionMethods
{
    // why it is not a good way to go?
    public static IDisposable SubscribeTo<T>(this IObserver<T> observer, IObservable<T> observable) => observable.Subscribe(observer);
}

...
        market.Subscribe(marketConsumer);
        // or
        //marketConsumer.SubscribeTo(market);
```

```cs
public static class ExtensionMethods
{
    // why it is not a good way to go?
    //public static IDisposable SubscribeTo<T>(this IObserver<T> observer, IObservable<T> observable) => observable.Subscribe(observer);

    public static IDisposable Inspect<T>(this IObservable<T> self, string name) =>
        self.Subscribe(
            x => Console.WriteLine($"{name} has generated value {x}"),
            ex => Console.WriteLine($"{name} has generated exception {ex.Message}"),
            () => Console.WriteLine($"{name} has completed")
            );

    public static IObserver<T> OnNext<T>(this IObserver<T> self, params T[] args)
    {
        foreach ( var arg in args )
            self.OnNext(arg);
        
        return self;
    }
}
```

```cs
using System.Reactive.Subjects;

namespace S10ProxyAndBroadcast;

internal class Program
{
    static void Main(string[] args)
    {
        var market = new Subject<float>(); // observable

        var marketConsumer = new Subject<float>(); // observer of market
                                                   // observable (Inspect)
        market.Subscribe(marketConsumer);

        marketConsumer.Inspect("market consumer");
        
        market.OnNext(1, 2, 3, 4);
        market.OnCompleted();
    }
}
```

## ReplaySubject [11.]

```cs
using System.Reactive.Subjects;

namespace S11ReplaySubject;

internal class Program
{
    static void Main(string[] args)
    {
        //1
        //var market = new Subject<float>();
        //market.Subscribe(x => Console.WriteLine($"Got the price {x}"));
        //market.OnNext(123);
        //Got the price 123

        //2
        //var market = new Subject<float>();
        //market.OnNext(123);
        //market.Subscribe(x => Console.WriteLine($"Got the price {x}"));
        //// NO OUTPUT

        //3
        //var market = new ReplaySubject<float>();
        //market.OnNext(123);
        //market.Subscribe(x => Console.WriteLine($"Got the price {x}"));
        //Got the price 123

        //4 Time Window
        var timeWindow = TimeSpan.FromMilliseconds(500);
        var market = new ReplaySubject<float>(timeWindow);
        
        market.OnNext(123);
        Thread.Sleep(200);
        market.OnNext(456);
        Thread.Sleep(200);
        market.OnNext(789);
        Thread.Sleep(200);

        market.Subscribe(x => Console.WriteLine($"Got the price {x}"));
        // Just two messages
        //Got the price 456
        //Got the price 789

        //5. Buffer size
        var marketB = new ReplaySubject<float>(2);

        marketB.OnNext(123);
        marketB.OnNext(456);
        marketB.OnNext(789);
        marketB.Subscribe(x => Console.WriteLine($"Got the price {x}"));
        // Just two messages
        //Got the price 456
        //Got the price 789
    }
}
```

## BehaviorSubject [12.]

<https://learn.microsoft.com/en-us/previous-versions/dotnet/reactive-extensions/hh211949(v=vs.103)#constructors>


```cs
using System.Reactive.Subjects;
using Shared;

namespace S12BehaviorSubject;

// you want expose the sensor
public class Scada
{
    private BehaviorSubject<double> sensorValue;

    public IObservable<double> SensorValue => sensorValue;  
}

internal class Program
{
    static void Main(string[] args)
    {
        var sensorReading = new BehaviorSubject<double>(-1.0);
        // Initializes a new instance of the BehaviorSubject<T> class which creates a subject that caches its last value and starts with the specified value.
        
        sensorReading.Inspect("sensor");
        
        sensorReading.OnNext(0.99);
        
        sensorReading.OnCompleted();

//sensor has generated value -1
//sensor has generated value 0.99
//sensor has completed

    }
}
```

## AsyncSubject [13.]

Represents the result of an asynchronous operation.

```cs
using System.Reactive.Subjects;
using Shared;

namespace S13AsyncSubject;

internal class Program
{
    static void Main(string[] args)
    {
        //Task<int> task = Task.Run(() => 42);
        //int t = task.Result;
        //similar
        var sensor = new AsyncSubject<double>();
        sensor.Inspect("async");

        sensor.OnNext(1.0);
        sensor.OnNext(2.0);

        // if we run at that point => no output because Completed is not called

        // but if we call:
        sensor.OnCompleted();

        // We will have the last value sent into receiver:
        //Output:
        //async has generated value 2
        //async has completed

        sensor.OnNext(123);//after the OnCompleted call, according to the contract, no exception but it will not do anything
        sensor.OnCompleted();
    }
}
```

## Implementing IObservable [14.]

```cs
using System.Collections.Immutable;
using System.Reactive.Disposables;
using Shared;

namespace S14ImplementingIObservable
{
    public class Market : IObservable<float>
    {
        // Keep a list of all components which are observing you ?
        // What do you use? List?
        // HashSet to prevent duplicates?
        // ConcurrentBag for Thread-Safety?

        private ImmutableHashSet<IObserver<float>> observers =
            ImmutableHashSet<IObserver<float>>.Empty;

        //public class Subscription : IDisposable
        //{
        //    public void Dispose()
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        public IDisposable Subscribe(IObserver<float> observer)
        {
            observers = observers.Add(observer);
            return Disposable.Create(
                dispose: () => {
                    observers = observers.Remove(observer);
                });
        }

        public void Publish(float price)
        {
            foreach(var o in observers)
            {
                o.OnNext(price);
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var market = new Market();
            var subscription = market.Inspect("market");

            market.Publish(123.0f); // market has generated value 123

            subscription.Dispose();

            market.Publish(321.0f); // no, already disposed
        }
    }
}
```

## Summary [15.]

Subject < T > is both IObservable and IObserver

- A subject can thus act as a proxy between an observer and observable.

Unsubscription via IDisposable

- Subscribe() return IDisposable

**ReplaySubject** - caches and replays all the values to any new subscriber

**BehaviorSubject** - has a default value that is sent if no other value is provided **and** the sequence is completed

**AsyncSubject** - stores the last value and publishes it when it completed

## Section 3 Fundamental Sequence Operators Overview [16.]

- Simple Factory Methods
- Observable.Create
- single value or sequence
- Sequence Generators
- Convert existing constructs into Observables
- Sequence Filtering
- Sequence Inspection
- Sequence Transformation
- Sequence Aggregation

## Simple Factory Methods [17.]

## Observable.Create [18.]

## Sequence Generators [19.]

## Converting Into Observables [20.]

## Sequence Filtering [21.]

## Sequence Inspection [22.]

## Sequence Transformation [23.]

## Sequence Aggregation [24.]

## Quiz 3: Fundamental Sequence Operators [  ]

## Summary [25.]

## Overview [26.]

## Exception Handling [27.]

## Sequence Combinators [28.]

## Time-Related Sequence Processing [29.]

## Reactive Extensions Event Broker [30.]

## Quiz 4: Advanced Sequence Operators [  ]

## Summary [31.]

## Course Summary [32.]

## Bonus Lecture: Other Courses at a Discount [33.]

