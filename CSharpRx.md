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
        - [Observable.Start](#observablestart)
        - [Observable.FromEventPattern](#observablefromeventpattern)
        - [Task.ToObservable](#tasktoobservable)
        - [IEnumerable.ToObservable](#ienumerabletoobservable)
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

Observable

- Return()
- Empty()
- Never()
- Throw()

```cs
        //var obs = Observable.Return<int>(42);
        var obs = Observable.Return(42); // ReplaySubject
        obs.Inspect("obs"); // obs has generated value 42
                            // obs has completed

        var obsEmpty = Observable.Empty<int>(); 
        // doesn't produce the value but produces completion signal
        obsEmpty.Inspect("obsEmpty"); // obsEmpty has completed

        var obsNever = Observable.Never<int>(); // no value, no signal
        obsNever.Inspect("obsNever");

        var obsThrow = Observable.Throw<int>(new Exception("oops"));
        obsThrow.Inspect("obsThrow");
        // obsThrow has generated exception oops
```

## Observable.Create [18.]

```cs
namespace S18ObservableCreate;

using Shared;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Timers;

internal class Program
{
    private static IObservable<string> Blocking()
    {
        var subject = new ReplaySubject<string>();
        subject.OnNext("foo", "bar");
        subject.OnCompleted();
        
        Thread.Sleep(3000);
        
        return subject;
    }

    private static IObservable<string> NonBlocking()
    {
        return Observable.Create<string>(observer =>
        {
            observer.OnNext("foo", "bar");
            observer.OnCompleted();

            Thread.Sleep(3000);
            
            return Disposable.Empty;
        });
    }

    /*
    public static IObservable<T> Return<T>(T value)
    {
        return Observable.Create<T>(x =>
        {
            x.OnNext(value);
            x.OnCompleted();
            return Disposable.Empty;
        });
    }
    */

    static void Main(string[] args)
    {
        Blocking().Inspect("blocking"); // waits 3 s. than we have our values

        NonBlocking().Inspect("nonblocking");

        //var meaningOfLife = Observable.Return<int>(42); // OnNext(42), OnCompleted

        var obs = Observable.Create<string>(o =>
        {
            var timer = new Timer(1000);
            timer.Elapsed += (sender, e) => o.OnNext($"tick {e.SignalTime}");
            timer.Elapsed += TimerOnElapsed;
            timer.Start();

            //return Disposable.Empty;
            // Yes, this tick-tock issue is not easy !!!
            // if we return Disposable.Empty
            // - we are not getting the ticks,
            // BUT we are getting the tocks
            // How?
            // The timer isn't destroyed, second subscription (TimerOnElapsed) is working
            // The first one, where you have an Observable OnNext, that's have been killed,
            // But the timer is still around
            // - How to get rid of that?
            // - return the action which being performed whenever somebody close the subscription
            // => so unsubscribe and kill the timer:

            return () =>
            {
                timer.Elapsed -= TimerOnElapsed;
                timer.Dispose();
            };
        });

        var sub = obs.Inspect("timer");

        Console.ReadLine();
        sub.Dispose();
        
        Console.ReadLine();
    }

    private static void TimerOnElapsed(object? sender, ElapsedEventArgs args)
    {
        Console.WriteLine($"tock {args.SignalTime}");
    }
}

```

## Sequence Generators [19.]

- Observable.Range
- Observable.Generate
- Observable.Interval
- Observable.Timer


```cs
using Shared;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace S19SequenceGenerators
{
    internal class Program
    {
        public static void MsTestInterval()
        {
            //https://learn.microsoft.com/en-us/previous-versions/dotnet/reactive-extensions/hh228911(v=vs.103)

            //*********************************************************************************************//
            //*** Generate a sequence of integers starting at zero until ENTER is pressed.              ***//
            //***                                                                                       ***//
            //*** A new integer will be generated by the Interval operator after each 2 second period   ***//
            //*** expires.                                                                              ***//
            //***                                                                                       ***//
            //*** By using the ThreadPool scheduler, the sequence of integers will be generated by a    ***//
            //*** thread in the .NET thread so the main thread is not blocked.                          ***//
            //*********************************************************************************************//

            const int periodInSec = 2;
            var obs = Observable.Interval(TimeSpan.FromSeconds(periodInSec),
                /*Scheduler.ThreadPool*/Scheduler.Default);

            //Warning CS0618  'Scheduler.ThreadPool' is obsolete:
            //'This property is no longer supported due to refactoring of the API surface and elimination of platform-specific dependencies.
            //Consider using Scheduler.Default to obtain the platform's most appropriate pool-based scheduler.
            //In order to access a specific pool - based scheduler,
            //please add a reference to the System.Reactive.PlatformServices assembly for your target platform
            //and use the appropriate scheduler in the System.Reactive.Concurrency namespace.'

            //*********************************************************************************************//
            //*** Write each value from Interval to the console window along with the current time to   ***//
            //*** show the period time span in effect.                                                  ***//
            //*********************************************************************************************//

            using (IDisposable handle = obs.Subscribe(x => Console.WriteLine("Integer : {0}\tCurrent Time : {1}", x, DateTime.Now.ToLongTimeString())))
            {
                Console.WriteLine("Press ENTER to exit...\n");
                Console.ReadLine();
            }
        }

        static void Main(string[] args)
        {
            var tenToTwenty = Observable.Range(10, 11);
            var tenToTwenty2 = Observable.Range(10, 11).Select(v => $"[{v}]");
            tenToTwenty.Inspect("range");
            tenToTwenty2.Inspect("range2");

            var generated = Observable.Generate(
                1,
                value => value < 100,
                value => value * value + 1,
                value => $"[{value}]" // like LINQ Select()
                );
            generated.Inspect("generated");

            var generated2 = Observable.Generate(
                initialState: 1,
                condition: value => value < 100,
                iterate: value => value * value + 1,
                resultSelector: value => $"[{value}]");
            generated2.Inspect("generated");

            var interval = Observable.Interval(TimeSpan.FromMilliseconds(500));
            using (interval.Inspect("interval"))
            {
                Console.ReadKey();
            }

            MsTestInterval();

            Console.WriteLine("Wait for the timer, 2 sec:");
            var timer = Observable.Timer(TimeSpan.FromSeconds(2));
            timer.Inspect("timer"); // wait single interval and produce the value
            Console.ReadLine();

            //Replicate Observable.Interval with Observable.Timer
            Console.WriteLine("Wait for the timer2, 2 sec with the period 2 sec, similar to the Interval:");
            var timer2 = Observable.Timer(
                dueTime: TimeSpan.FromSeconds(2),
                period: TimeSpan.FromSeconds(2) // specify period = similar to Interval
                /* or TimeSpan.Zero with no delay*/);
            timer2.Inspect("timer2");
            Console.ReadLine();
        }
    }
}
```

## Converting Into Observables [20.]

- How to convert existing paradigm ( for ex, using delegates) - to using reactive expression ?

How to run the Task is separate Thread and make it observable ?

### Observable.Start

```cs
using System.Reactive.Linq;
using Shared;

namespace S20ConvertingIntoObservables;

internal class Program
{
    static void Main(string[] args)
    {
        TestObservableStart();
    }

    static void ThreadProc(Object stateInfo)
    {
        // No state object was passed to QueueUserWorkItem, so stateInfo is null.
        Console.WriteLine("Hello from the thread pool.");
    }

    static void TestObservableStart()
    {
        //https://learn.microsoft.com/en-us/dotnet/api/system.threading.threadpool.queueuserworkitem?view=net-7.0
        // Queue the task.
        ThreadPool.QueueUserWorkItem(ThreadProc);
        Console.WriteLine("Main thread does some work, then sleeps.");
        Thread.Sleep(1000);

        Console.WriteLine("Main thread exits.");

        //Similar with Observable:

        var start = Observable.Start(() => {
            Console.WriteLine("Starting work... ");
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(200);
                Console.Write(".");
            }
        });

        for (int i = 0; i < 10; i++)
        {
            Thread.Sleep(200);
            Console.Write("-");
        }

        start.Inspect("start");
        Console.ReadKey();
    }

//Main thread does some work, then sleeps.
//Hello from the thread pool.
//Main thread exits.


//Starting work...
//-.-..--..--..--..--.start has generated value()
//start has completed

}
```

### Observable.FromEventPattern

```cs
    public class Market
    {
        private float price;
        public float Price
        {
            get => price;
            set => price = value;
        }

        public void ChangePrice(float price)
        {
            Price = price;
            PriceChanged?.Invoke(this, price);
        }

        public event EventHandler<float> PriceChanged;
    }
    
    static void TestFromEventPattern()
    {
        var market = new Market();
        var priceChanges = Observable.FromEventPattern<float>
            (
                h => market.PriceChanged += h,
                h => market.PriceChanged -= h
            );

        //priceChanges.Inspect("price changes");
        priceChanges.Subscribe (x => Console.WriteLine(x.EventArgs));

        market.ChangePrice(1.5f);
        market.ChangePrice(2.4f);
        market.ChangePrice(3.2f);
        //Output with our Inspect():
        //price changes has generated value System.Reactive.EventPattern`1[System.Single]
    }
```

### Task.ToObservable()

```cs
    var task = Task.Factory.StartNew(() => "Test");
    var source = task.ToObservable();
    source.Inspect("task");
    Console.ReadLine();
```

### IEnumerable.ToObservable()

```cs
    var items = new List<int> { 1, 2, 3 };
    var source = items.ToObservable();
    source.Inspect("ItemsToObservable");
```

## Sequence Filtering [21.]

- Where()
- Distinct()
- DistinctUntilChanged()
- IgnoreElements()
- Skip()
- Take()
- SkipWhile()
- TakeWhile()
- SkipLast()
- SkipUntil()

```cs
        Observable.Range(0, 100)
            .Where(i => i % 9 == 0)
            .Inspect("where");

        var values = Observable.Range(-10, 21);
        values
            .Select(i => i * i)
            .Distinct()
            .Inspect("select distinct");

        new List<int> { 1, 1, 2, 2, 3, 3, 2, 2 }
        .ToObservable()
        .DistinctUntilChanged() //looks on previous value
        //.IgnoreElements()
        .Inspect("DistinctUntilChanged");

        Observable.Range(1, 10)
            .Skip(3)
            .Take(4)
            .Inspect("Skip3Take4");

        Observable.Range(-10, 21)
            .SkipWhile(x => x < 0)
            .TakeWhile(x => x < 6)
            .Inspect("SkipWhileTakeWhile");

        Observable.Range(-10, 21)
            .SkipLast(5) 
            .Inspect("SkipLast");
        // problematic because you have to know how many elements do you have

        //values.SkipUntil()

        var stockPrices = new Subject<float>();
        var optionPrices = new Subject<float>();

        stockPrices.SkipUntil(optionPrices)
            .Inspect("skipuntil");

        stockPrices.OnNext(1);
        stockPrices.OnNext(2);
        stockPrices.OnNext(3);
        optionPrices.OnNext(55);
        stockPrices.OnNext(4, 5, 6); // only these will be available
        //skipuntil has generated value 4
        //skipuntil has generated value 5
        //skipuntil has generated value 6
```

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

