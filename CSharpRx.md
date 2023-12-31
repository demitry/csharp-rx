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
        - [Select, OfType, Cast, Timestamp, TimeInterval](#select-oftype-cast-timestamp-timeinterval)
        - [Materialization Debugging Info](#materialization-debugging-info)
        - [Collapsing sequence of sequences into 1 sequence](#collapsing-sequence-of-sequences-into-1-sequence)
    - [Sequence Aggregation [24.]](#sequence-aggregation-24)
        - [Count, Min, Max, Average](#count-min-max-average)
        - [FirstAsync and FirstOrDefaultAsync](#firstasync-and-firstordefaultasync)
        - [SingleAsync, SingleOrDefaultAsync](#singleasync-singleordefaultasync)
        - [Aggregate](#aggregate)
        - [Running, continuous Sum values - Scan](#running-continuous-sum-values---scan)
    - [Quiz 3: Fundamental Sequence Operators [  ]](#quiz-3-fundamental-sequence-operators---)
    - [Summary [25.]](#summary-25)
    - [Overview [26.]](#overview-26)
    - [Exception Handling [27.]](#exception-handling-27)
    - [Sequence Combinators [28.]](#sequence-combinators-28)
    - [Time-Related Sequence Processing [29.]](#time-related-sequence-processing-29)
    - [Reactive Extensions Event Broker [30.]](#reactive-extensions-event-broker-30)
    - [Quiz 4: Advanced Sequence Operators](#quiz-4-advanced-sequence-operators)
        - [Question 1](#question-1)
        - [Question 2](#question-2)
        - [Question 3](#question-3)
    - [Summary [31.]](#summary-31)
    - [Course Summary [32.]](#course-summary-32)

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

- Any()
- All()
- Contains()
- DefaultIfEmpty()
- ElementAt()
- SequenceEqual()

```cs
        var subject = new Subject<int>();
        subject.Any(x => x > 1).Inspect("any");
        subject.OnCompleted();
        //any has generated value False
        //any has completed

        //We haven't this at LINQ but have in Reactive Extensions
        // Sequence MUST be completed (subject.OnCompleted(); is called)
        // In the other case we cannot make a decision about these values.

        //subject.OnNext(2);

        var values = new List<int> {/* -1, -2, 0,*/ 1, 2, 3, 4, 5 };
        values.ToObservable()
            .All(x => x > 0)
            .Inspect("all"); 
        //all has generated value True
        //all has completed

        var subj = new Subject<string>();
        subj.Contains("foo").Inspect("contains");
        subj.OnNext("foo");
        //contains has generated value True
        //contains has completed

        // I want to get notifications from this observable
        // but if it has no values, please give me some kind of default
        // DefaultIfEmpty() provides you default value if sequence generated no values
        // but the sequence must be completed
        var subjB = new Subject<float>();
        subjB.DefaultIfEmpty(0.99f)
            .Inspect("subjB");
        //subjB.OnCompleted(); //subjB has generated value 0.99, subjB has completed
        //or
        subjB.OnNext(1.2f); //subjB has generated value 1.2

        var numbers = Observable.Range(0, 10);
        numbers.ElementAt(5)
            .Inspect("ElementAt");
        //ElementAt has generated value 5
        //ElementAt has completed

        var numbers2 = Observable.Range(0, 10);
        numbers2.ElementAt(15)
            .Inspect("ElementAt15");
        // ElementAt15 has generated exception Specified argument was out of the range of valid values. (Parameter 'index')
        //remember Inspect():
        //ex => Console.WriteLine($"{name} has generated exception {ex.Message}")

        var seq1 = new Subject<int>();
        var seq2 = new Subject<int>();

        seq1.Inspect("seq1");
        seq2.Inspect("seq2");

        seq1.SequenceEqual(seq2)
            .Inspect("SequenceEqual");

        //seq1.OnNext(1);
        //seq2.OnNext(2);
        //SequenceEqual has generated value False
        //SequenceEqual has completed

        seq1.OnNext(1);
        seq2.OnNext(1);
        seq1.OnCompleted();
        seq2.OnCompleted();
        //seq1 has generated value 1
        //seq2 has generated value 1
        //seq1 has completed
        //seq2 has completed
        //SequenceEqual has generated value True
        //SequenceEqual has completed
```

## Sequence Transformation [23.]

### Select, OfType, Cast, Timestamp, TimeInterval

- Select()
- OfType()
- Cast()
- Timestamp()
- TimeInterval()

```cs
        var numbers = Observable.Range(1, 10);
        numbers.Select(x => x * x).Inspect("select");

        var subj = new Subject<object>();
        subj.OfType<float>().Inspect("OfTypeFloat");
        // filter and get all floats
        subj.Cast<float>().Inspect("CastFloat");
        // try to cast all to float and throw an exception if cast is impossible
        subj.OnNext(1.0f, 2, 3.0);

        var seq = Observable.Interval(TimeSpan.FromSeconds(1));
        //seq.Timestamp().Inspect("Timestamp");
        //Timestamp has generated value 0@7/25 / 2023 5:17:20 PM +00:00
        //Timestamp has generated value 1@7/25 / 2023 5:17:21 PM +00:00
        //Timestamp has generated value 2@7/25 / 2023 5:17:22 PM +00:00
        //...
        // Current timestamp

        //seq.TimeInterval().Inspect("TimeInterval");
        //TimeInterval has generated value 0@00:00:01.0262434
        //TimeInterval has generated value 1@00:00:01.0049405
        //...
        //This value shows us how much time was elapsed after the previous event was generated (in average = 1 sec)

        Console.ReadLine();
```

### Materialization Debugging Info

```cs
    static void Main(string[] args)
    {
        // When we are talking about materialization/dematerialization
        // in LINQ terminology, when materialize a IEnumerable -
        // what you really do - a converting to a List or another collection
        // In Reactive Extensions materialization/dematerialization is 
        // ABSOLUTELY DIFFERENT THING:
        // You generate some kind of debugging information for a particular sequence:

        var seq = Observable.Range(0, 4);
        seq.Materialize().Inspect("Materialize");
        Console.ReadLine();

        //Materialize has generated value OnNext(0)
        //Materialize has generated value OnNext(1)
        //Materialize has generated value OnNext(2)
        //Materialize has generated value OnNext(3)
        //Materialize has generated value OnCompleted()
        //Materialize has completed

        // Instead of giving us values 0,1,2,3 it gives
        // "value" OnNext(), OnCompleted()

        // public static IObservable<Notification<TSource>> Materialize<TSource>(this IObservable<TSource> source)
        //public static IObservable<Notification<int>> Materialize<int>(this IObservable<int> source)

        /*
        Notification<int> notification;
        switch (notification.Kind)
        {
            case NotificationKind.OnCompleted: break;
            case NotificationKind.OnError: break;
            case NotificationKind.OnNext: break;
        }
        notification.Value
        */

        //Dematerialize() = 
        // gets you back from notifications of int to ordinary observable sequence

        var seq2 = Observable.Range(0, 4);
        seq2.Materialize()
            .Dematerialize()
            .Inspect("Dematerialize");
        Console.ReadLine();

        /*
        Materialize has generated value OnNext(0)
        Materialize has generated value OnNext(1)
        Materialize has generated value OnNext(2)
        Materialize has generated value OnNext(3)
        Materialize has generated value OnCompleted()
        Materialize has completed
        b
        Dematerialize has generated value 0
        Dematerialize has generated value 1
        Dematerialize has generated value 2
        Dematerialize has generated value 3
        Dematerialize has completed
        */
    }
```

### Collapsing sequence of sequences into 1 sequence

```cs
        // 1  1 2  1 2 3  1 2 3 4
        //var seq = Observable.Range(1, 4)
        //    .SelectMany(x => Observable.Range(1, x))
        //    .Inspect("SelectMany");
        /*
        SelectMany has generated value 1
        SelectMany has generated value 1
        SelectMany has generated value 2
        SelectMany has generated value 1
        SelectMany has generated value 2
        SelectMany has generated value 1
        SelectMany has generated value 3
        SelectMany has generated value 2
        SelectMany has generated value 3
        SelectMany has generated value 4
        SelectMany has completed
        */

        // You cannot expect that everything will be exact in order
        // 

        // You can specify 
        //var seq2 = Observable.Range(1, 4, 
        //    scheduler: Scheduler.Immediate)
        //.SelectMany(x => Observable.Range(1, x))
        //.Inspect("SelectManySchedulerImmediate");

        /*
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 2
        SelectManySchedulerImmediate has generated value 2
        SelectManySchedulerImmediate has generated value 2
        SelectManySchedulerImmediate has generated value 3
        SelectManySchedulerImmediate has generated value 3
        SelectManySchedulerImmediate has generated value 4
        SelectManySchedulerImmediate has completed
         */

        var seq3 = 
            Observable.Range(1, 4, scheduler: Scheduler.Immediate)
            .SelectMany(x => Observable.Range(1, x, scheduler: Scheduler.Immediate))
            .Inspect("SelectManySchedulerImmediate");

        /*
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 2
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 2
        SelectManySchedulerImmediate has generated value 3
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 2
        SelectManySchedulerImmediate has generated value 3
        SelectManySchedulerImmediate has generated value 4
        SelectManySchedulerImmediate has completed 
        */
```

## Sequence Aggregation [24.]

### Count(), Min(), Max(), Average()

- Count()
- Min()
- Max()
- Average()

```cs
        // Collapsing reactive sequence
        var values = Observable.Range(1, 5);
        values.Inspect("Values");
        values.Count().Inspect("count");
        /*
        Values has generated value 1
        Values has generated value 2
        Values has generated value 3
        Values has generated value 4
        Values has generated value 5
        Values has completed
        count has generated value 5
        count has completed
        */

        var intSubj = new Subject<int>();
        intSubj.Inspect("intSubj");
        intSubj.Min().Inspect("min");
        intSubj.Max().Inspect("max");
        intSubj.Average().Inspect("avg"); // Average() returns double

        intSubj.OnNext(2);
        intSubj.OnNext(4);
        intSubj.OnNext(5);
        intSubj.OnNext(12);

        intSubj.OnCompleted(); // NB!

        /*
            intSubj has generated value 2
            intSubj has generated value 4
            intSubj has generated value 5
            intSubj has generated value 12
            intSubj has completed
            min has generated value 2
            min has completed
            max has generated value 12
            max has completed
            avg has generated value 5.75
            avg has completed  
         */
```

### FirstAsync() and FirstOrDefaultAsync()

```cs
        var replay = new ReplaySubject<int>();

        replay.OnNext(-1);
        replay.OnNext(2);
        replay.OnCompleted(); // it is replay subj so san subscribe after

        replay.FirstAsync(i => i > 0) // First() is deprecated
            .Inspect("FirstAsync");

        //FirstAsync has generated value 2
        //FirstAsync has completed

        //return Observable that yields the actual value
        //public static IObservable<TSource> FirstAsync<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)

        // What is we have no value > 0 ?
        var replay2 = new ReplaySubject<int>();
        replay2.OnNext(-1);
        replay2.OnCompleted();

        //replay2.FirstAsync(i => i > 0).Inspect("FirstAsync2");

        //FirstAsync2 has generated exception Sequence contains no matching element.
        // The same behavior in LINQ

        replay2.FirstOrDefaultAsync(i => i > 0).Inspect("FirstAsync2");
        //Default for int is 0, so
        //FirstAsync2 has generated value 0
        //FirstAsync2 has completed
```

### SingleAsync, SingleOrDefaultAsync

```cs
        var replay = new ReplaySubject<int>();
        replay.OnNext(2);
        replay.OnCompleted();
        replay.SingleAsync().Inspect("SingleAsync");
        //SingleAsync has generated value 2
        //SingleAsync has completed

        // With the single value it is OK
        // BUT
        // if Sequence contains more than one element. => exception
        var replay2 = new ReplaySubject<int>();
        replay2.OnNext(1);
        replay2.OnNext(2);
        replay2.OnCompleted();
        replay2.SingleAsync().Inspect("SingleAsync2");
        // SingleAsync2 has generated exception Sequence contains more than one element.

        var replay3 = new ReplaySubject<int>();
        replay3.OnCompleted();
        replay3.SingleOrDefaultAsync().Inspect("SingleOrDefaultAsync");
        //SingleOrDefaultAsync has generated value 0
        //SingleOrDefaultAsync has completed
```

### Aggregate

```cs
        var replay = new ReplaySubject<int>();
        replay.OnNext(-1);
        replay.OnNext(2);
        replay.OnCompleted();
        replay.Sum().Inspect("Sum");
        //Sum has generated value 1
        //Sum has completed

        // What if "running", "continuous" Sum() ?
        // What if we want to comment replay.OnCompleted();
        // What if we want to remove restriction OnCompleted

        var subject = new Subject<double>();
        int power = 1;

        subject.Aggregate(
             0.0, 
             (partialResult, currentValue) => 
                partialResult + Math.Pow(currentValue, partialResult++))
            .Inspect("poly");

        // 1, 2, 4
        // 1^1 + 2^2 + 4^3

        subject.OnNext(1, 2, 4).OnCompleted();
```

### Running, continuous Sum values - Scan()

```cs
        var subject = new Subject<double>();
        subject.Scan(0.0, (p, c) => p + c).Inspect("scan");
        subject.OnNext(1, 2, 3, 4);
        //scan has generated value 1
        //scan has generated value 3
        //scan has generated value 6
        //scan has generated value 10
```

## Quiz 3: Fundamental Sequence Operators [  ]

## Summary [25.]

- Observable.Return = single value + completion
- Observable.Empty = NO value + completion
- Observable.Never = NO value + NO completion
- Observable.Throw = calls OnError() with an exception
- Observable.Create
  - creates a nonblocking stream
  - Returns an IDisposable
- Use the Disposable helper class
  - Disposable.Empty - does nothing
  - Disposable.Create - takes an unsubscribe action
  
- Observable.Range = set of int values + completion
- Observable.Generate = custom generation
- Observable.Interval = incremental long values every time period
- Observable.Timer takes 2 args:
  - Initial time delay
  - Delay between calls
  - Acts as Observable.Interval, generates consecutive long values every time period

- Observable.Start = invoke delegate as Observable
- Observable.FromEventPattern = use .NET events
- ToObservable() = Convert a Task or IEnumerable to Observable
- Other paradigms (INotifyPropertyChanged) supported in Rxx Nuget package
- Rxx = Extensions for Reactive Extensions

- Filtering operations:
  - Where, Distinct, DistinctUntilChanged, IgnoreElements, Skip, Take/TakeWhile, Skip/SkipWhile, SkipLast, SkipUntil

- Inspection operations:
  - Any, All, Contains, DefaultIfEmpty, ElementAt, SequenceEqual, First/Last/Single(OrDefault)Async

- Transformation operations:
  - Select, OfType/Cast, Materialize/Dematerialize, SelectMAny

- Aggregation operations:
  - Count, Sum, Min, Max, Average,..., Aggregate, Scan


## Overview [26.]

- Exception Handling - it is not rty catch, different
- Sequence Combinators - like LINQ
- Time-Related Sequence Processing
- Reactive Extensions Event Broker - Paractical Example with DI

## Exception Handling [27.]

Our Inspect() catches exceptions, but if we want catch them and handle...

```cs
using Shared;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace S27ExceptionHandling;

internal class Program
{
    static void Main(string[] args)
    {
        //TestCatchWithFallback();
        //TestCatchWithFallbackEmpty();
        //TestCatchDifferentEx();
        //TestOnErrorResumeNext();
        TestRetry();
    }

    static void TestCatchWithFallback()
    {
        var subject = new Subject<int>();
        var fallback = Observable.Range(1, 3);

        subject
            .Catch(fallback)
            .Inspect("subject");

        subject.OnNext(32);
        subject.OnError(new Exception("Ooops"));

        //subject has generated value 32
        //subject has generated value 1
        //subject has generated value 2
        //subject has generated value 3
        //subject has completed
    }

    static void TestCatchWithFallbackEmpty()
    {
        var subject = new Subject<int>();

        subject
            .Catch(Observable.Empty<int>())
            .Inspect("subject");

        subject.OnNext(32);
        subject.OnError(new Exception("Ooops"));

        //subject has generated value 32
        //subject has completed
    }

    static void TestCatchDifferentEx()
    {
        var subject = new Subject<int>();

        subject
            .Catch<int, ArgumentException>(
              ex => Observable.Return(111)
            )
            .Catch(Observable.Empty<int>())
            .Inspect("subject");

        subject.OnNext(32);
        subject.OnError(new ArgumentException("arg"));
        subject.OnError(new Exception("Ooops"));

        //subject has generated value 32
        //subject has completed
    }

    static void TestOnErrorResumeNext()
    {
        var seq1 = new Subject<char>();
        var seq2 = new Subject<char>();

        seq1.OnErrorResumeNext(seq2).Inspect("OnErrorResumeNext");
        // We don't care, just continue with other sequence

        //abcdef

        seq1.OnNext('a', 'b', 'c')
            .OnError(new Exception());

        seq2.OnNext('d', 'e', 'f')
            .OnCompleted();
    }


    private static IObservable<int> SucceedAfter(int attempts)
    {
        int count = 0;
        return Observable.Create<int>(o =>
        {
            Console.WriteLine((count > 0 ? "Ret" : "T") + "rying to do work");
            if(count++ < attempts)
            {
                Console.WriteLine("Failed");
                o.OnError(new Exception());
            }
            else 
            { 
                Console.WriteLine("Succeeded");
                o.OnNext(count);
                o.OnCompleted();
            }

            return Disposable.Empty;
        });
    }

    static void TestRetry()
    {
        SucceedAfter(3).Retry(4).Inspect("Retry");

        /*
        Trying to do work
        Failed
        Retrying to do work
        Failed
        Retrying to do work
        Failed
        Retrying to do work
        Succeeded
        Retry has generated value 4
        Retry has completed
        */
    }
}
```

## Sequence Combinators [28.]

- CombineLatest
- Zip
- And-Then-When (Zip Multiple Sequences)
- Concat
- Repeat
- StartWith
- Amb(iguous)
- Merge

```cs
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Shared;

namespace S28SequenceCombinators;

//SequenceCombinators: CombineLatest, Zip, And-Then-When (Zip Multiple Sequences), Concat, Repeat, StartWith, Amb(iguous), Merge

internal class Program
{
    static void Main(string[] args)
    {
        TestCombineLatest();

        TestZip();

        TestZipThreeSequences();

        TestConcatRepeatStartWith();

        TestAmbiguous();

        TestMerge();
    }

    static void TestCombineLatest()
    {
        var mechanical = new BehaviorSubject<bool>(true);
        var electrical = new BehaviorSubject<bool>(true);
        var electronic = new BehaviorSubject<bool>(true);

        mechanical.Inspect("mechanical");
        electrical.Inspect("electrical");
        electronic.Inspect("electronic");

        // True
        // True, what if False?
        // True

        Observable.CombineLatest(mechanical, electrical, electronic)
          .Select(values => values.All(x => x))
          .Inspect("Is the system OK?");

        // True

        mechanical.OnNext(false); //break system

        // => False
    }

    static void TestZip()
    {
        var digits = Observable.Range(0, 10);
        var letters = Observable.Range(0, 10).Select(x => (char)('A' + x));

        letters
            .Zip(digits, (letter, digit) => $"{letter}-{digit}")
            .Inspect("Zip");

        //Zip has generated value A-0
        //Zip has generated value B-1
        //Zip has generated value C-2
        //Zip has generated value D-3
        //Zip has generated value E-4
        //Zip has generated value F-5
        //Zip has generated value G-6
        //Zip has generated value H-7
        //Zip has generated value I-8
        //Zip has generated value J-9
        //Zip has completed
    }

    static void TestZipThreeSequences()
    {
        var digits = Observable.Range(0, 10);
        var letters = Observable.Range(0, 10).Select(x => (char)('A' + x));
        // And returns Pattern<T1, T2> whose properties are internal
        var punctuation = "!@#$%^&*()".ToCharArray().ToObservable();

        Observable.When( // return type is Plan<> and all that
            digits
              .And(letters)
              .And(punctuation)
              .Then((digit, letter, symbol) => $"{digit}-{letter}-{symbol}")
              )
            .Inspect("And-Then-When");
        /*
            And-Then-When has generated value 0-A-!
            And-Then-When has generated value 1-B-@
            And-Then-When has generated value 2-C-#
            And-Then-When has generated value 3-D-$
            And-Then-When has generated value 4-E-%
            And-Then-When has generated value 5-F-^
            And-Then-When has generated value 6-G-&
            And-Then-When has generated value 7-H-*
            And-Then-When has generated value 8-I-(
            And-Then-When has generated value 9-J-)
            And-Then-When has completed
         */
    }

    static void TestConcatRepeatStartWith()
    {
        // Concat merges all the sequences into one
        var s1 = Observable.Range(1, 3); // 1,2,3
        var s2 = Observable.Range(4, 3); // 4,5,6
        s2.Concat(s1).Inspect("Concat");
        //Concat has generated value 4
        //Concat has generated value 5
        //Concat has generated value 6
        //Concat has generated value 1
        //Concat has generated value 2
        //Concat has generated value 3
        //Concat has completed

        // Repeat repeats the sequence as often as is necessary
        s1.Repeat(3).Inspect("Repeat");
        //Repeat has generated value 1
        //Repeat has generated value 2
        //Repeat has generated value 3
        //Repeat has generated value 1
        //Repeat has generated value 2
        //Repeat has generated value 3
        //Repeat has generated value 1
        //Repeat has generated value 2
        //Repeat has generated value 3
        //Repeat has completed

        s1.StartWith(2, 1, 0).Inspect("StartWith"); // Adds values before 
        //StartWith has generated value 2
        //StartWith has generated value 1
        //StartWith has generated value 0
        //StartWith has generated value 1
        //StartWith has generated value 2
        //StartWith has generated value 3
        //StartWith has completed
    }

    static void TestAmbiguous()
    {
        // Amb(iguous)
        // will return a value from the sequence that first produces a value
        // will ignore values from all other sequences

        var seq1 = new Subject<int>();
        var seq2 = new Subject<int>();
        var seq3 = new Subject<int>();
        seq1.Amb(seq2).Amb(seq3).Inspect("Amb");
        //seq1.OnNext(1); // 1, 10, 100, but comment this out => 2,20,200
        seq2.OnNext(2);
        seq3.OnNext(3);
        seq1.OnNext(10);
        seq2.OnNext(20);
        seq3.OnNext(30);
        seq1.OnNext(100);
        seq2.OnNext(200);
        seq3.OnNext(300);
        seq1.OnCompleted();
        seq2.OnCompleted();
        seq3.OnCompleted();
        //Amb has generated value 1
        //Amb has generated value 10
        //Amb has generated value 100
        //Amb has completed
        // The first seq is used!
    }

    static void TestMerge()
    {
        // Merge pairs up values from multiple sequences
        // Gets info as soon as it comes in
        var foo = new Subject<long>();
        var bar = new Subject<long>();
        var baz = Observable.Interval(TimeSpan.FromMilliseconds(500)).Take(5);

        foo.Merge(bar).Merge(baz).Inspect("Merge");

        foo.OnNext(100);
        Thread.Sleep(1000);
        bar.OnNext(10);
        Thread.Sleep(1000);
        foo.OnNext(1000);
        Thread.Sleep(1000);

        //Merge has generated value 100
        //Merge has generated value 0
        //Merge has generated value 1
        //Merge has generated value 10
        //Merge has generated value 2
        //Merge has generated value 3
        //Merge has generated value 1000
        //Merge has generated value 4
    }
}
```

## Time-Related Sequence Processing [29.]

```cs
using Shared;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;

namespace S29TimeRelatedSeqProcessing;

internal class Program
{
    static void Main(string[] args)
    {
        //TestBuffer();

        //TestTimestamp();

        //TestDelay();

        //TestSample();

        TestKeyboardInputAndThrottleAndTimeout();
    }

    static void TestBuffer()
    {
        Observable.Range(1, 40)
            .Buffer(5)          //Buffer() - returns Observable<List<int>>
            .Subscribe(x => Console.WriteLine(string.Join(",", x)));

        /*
          1,2,3,4,5
          6,7,8,9,10
          11,12,13,14,15
            ...
          31,32,33,34,35
          36,37,38,39,40
        */
    }

    static void TestTimestamp()
    {
        Observable.Range(1, 40).Buffer(5, 3) // skip 1,2,3; than skip 4,5,6; ...
            .Subscribe(x => Console.WriteLine(string.Join(",", x)));
        /*
            1,2,3,4,5
            4,5,6,7,8
            7,8,9,10,11
            10,11,12,13,14
            ...
            34,35,36,37,38
            37,38,39,40
            40
         */

        var source = Observable.Interval(TimeSpan.FromSeconds(1))
            .Take(3);

        var delay = source.Delay(TimeSpan.FromSeconds(2));
        source.Timestamp().Inspect("source");
        delay.Timestamp().Inspect("delay");
    }

    static void TestDelay()
    {
        var source = Observable.Interval(TimeSpan.FromSeconds(1))
            .Take(3);

        var delay = source.Delay(TimeSpan.FromSeconds(2));
        source.Timestamp().Inspect("source");
        delay.Timestamp().Inspect("delay");

        Console.ReadLine();
        //source has generated value 0@7/30/2023 7:02:20 PM +00:00
        //source has generated value 1@7/30/2023 7:02:21 PM +00:00
        //source has generated value 2@7/30/2023 7:02:22 PM +00:00
        //source has completed
        //delay has generated value 0@7/30/2023 7:02:22 PM +00:00
        //delay has generated value 1@7/30/2023 7:02:23 PM +00:00
        //delay has generated value 2@7/30/2023 7:02:24 PM +00:00
        //delay has completed
    }

    static void TestSample()
    {
        var samples = Observable.Interval(TimeSpan.FromSeconds(0.5))
            .Take(20)
            .Sample(TimeSpan.FromSeconds(1.75));
        samples.Inspect("sample");
        samples.ToTask().Wait();

        //sample has generated value 2
        //sample has generated value 6
        //sample has generated value 9
        //sample has generated value 13
        //sample has generated value 16
        //sample has generated value 19
        //sample has completed
    }

    static void TestKeyboardInputAndThrottleAndTimeout()
    {
        var subj = new Subject<string>();
        subj
            //.Throttle(TimeSpan.FromSeconds(1))
            .Timeout(TimeSpan.FromSeconds(3))
            .Inspect("subj");

        string input = string.Empty;
        ConsoleKeyInfo c;
        while ((c = Console.ReadKey()).Key != ConsoleKey.Escape )
        {
            if(char.IsLetterOrDigit(c.KeyChar) ) 
            {
                input += c.KeyChar;
                subj.OnNext(input);
            }
        }
        //Hsubj has generated value H
        //esubj has generated value He
        //lsubj has generated value Hel
        //lsubj has generated value Hell
        //osubj has generated value Hello
    }
}

//TODO: Review
```

## Reactive Extensions Event Broker [30.]

```cs
using Autofac;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace S30RxEventBroker;

internal class Program
{
    public class Actor
    {
        protected EventBroker broker;

        public Actor(EventBroker broker)
        {
            this.broker = broker ?? throw new ArgumentNullException(nameof(broker));
        }
    }

    public class FootballPlayer : Actor
    {
        public string Name { get; set; }

        public int GoalsScored { get; set; } = 0;

        public void Score()
        {
            GoalsScored++;
            broker.Publish(new PlayerScoredEvent { Name = Name, GoalsScored = GoalsScored });
        }

        public void AssaultReferee()
        {
            broker.Publish(new PlayerSentOffEvent { Name = Name, Reason = "violence" });
        }

        public FootballPlayer(EventBroker broker, string name) : base(broker)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            broker.OfType<PlayerScoredEvent>()
                .Where(ps => !ps.Name.Equals(name))
                .Subscribe
                (
                    ps => Console.WriteLine($"{name}: Nicely done, {ps.Name}! It's your {ps.GoalsScored} goal.")
                );

            broker.OfType<PlayerSentOffEvent>()
                .Where(ps => !ps.Name.Equals(name))
                .Subscribe(
                ps => Console.WriteLine($"{name}: See you in the lockers, {ps.Name}!")
                );
        }
    }

    public class FootballCoach : Actor
    {
        public FootballCoach(EventBroker broker) : base(broker)
        {
            broker.OfType<PlayerScoredEvent>()
                .Subscribe(pe =>
                {
                    if (pe.GoalsScored < 3)
                        Console.WriteLine($"Coach: well done, {pe.Name}");
                });

            broker.OfType<PlayerSentOffEvent>()
                .Subscribe(pe =>
                {
                    Console.WriteLine($"Coach: well done, {pe.Name}");

                });
        }
    }

    public class PlayerEvent
    {
        public string Name { get; set; }
    }

    public class PlayerScoredEvent : PlayerEvent
    {
        public int GoalsScored { get; set; }
    }

    public class PlayerSentOffEvent : PlayerEvent
    {
        public string Reason { get; set; }
    }

    public class EventBroker : IObservable<PlayerEvent>
    {
        private Subject<PlayerEvent> subscriptions = new Subject<PlayerEvent>();

        IDisposable IObservable<PlayerEvent>.Subscribe(IObserver<PlayerEvent> observer)
        {
            return subscriptions.Subscribe(observer);
        }

        public void Publish(PlayerEvent pe)
        {
            subscriptions.OnNext(pe);
        }
    }

    static void Main(string[] args)
    {
        // Actor Pattern
        // Football Game
        // EventBroker = glue everything together
        // Propagate EventBroker everywhere with DI
        var cb = new ContainerBuilder();
        cb.RegisterType<EventBroker>().SingleInstance();
        cb.RegisterType<FootballCoach>();
        cb.Register((c, p) =>
            new FootballPlayer(
                c.Resolve<EventBroker>(), 
                p.Named<string>("name")
                ));
        using (var c = cb.Build())
        {
            var coach = c.Resolve<FootballCoach>();
            var player1 = c.Resolve<FootballPlayer>(new NamedParameter("name", "John"));
            var player2 = c.Resolve<FootballPlayer>(new NamedParameter("name", "Chris"));

            player1.Score();
            player1.Score();
            player1.Score(); //x
            player1.AssaultReferee();
            player2.Score();

            //Coach: well done, John
            //Chris: Nicely done, John!It's your 1 goal.
            //Coach: well done, John
            //Chris: Nicely done, John!It's your 2 goal.
            //Chris: Nicely done, John!It's your 3 goal.
            //Coach: well done, John
            //Chris: See you in the lockers, John!
            //Coach: well done, Chris
            //John: Nicely done, Chris!It's your 1 goal. // We can store and dispose the subscription there.
        }
    }
}
```

## Quiz 4: Advanced Sequence Operators

### Question 1

Sequence A  generates the values {a,b}  and then results in an error. Sequence B generates the values {c,d} . What sequence is generated by A.OnErrorResumeNext(B) ?

A: {a,b,c,d}

### Question 2

Consider sequences A = { 1,2,3 }, B = { 4,5,6 } and C = { 7,8,9 }. 
What is the result of Observable.CombineLatest(A,B,C).Select(z => z.Sum()) ?

A: 18

### Question 3

A reactive sequence X generates a value every second. If I wait for 10 seconds, how many values am I likely to receive with X.Sample(TimeSpan.FromSeconds(2)) ?

A: 5

## Summary [31.]

- **Catch (fallback)** continues with the fallback sequence on error in main sequence
- **Catch<T, MyException>** catches MyException and continues with the provided sequence
  - Use **Observable.Empty** to terminate
- **Finally()** executes regardless of exception
- **Retry(N)**, when an error is hit, unsubscribes and resubscribes a certain N number of times
- **Concat** waits for sequences to complete and bundles them together
- **Repeat(N)** repeats a completed sequence a number of times
- **StartWith(1,2,3)** prepends a sequence with values
- **Amb()** waits on several sequences and takes the one which produces a value first
- **Merge()** takes values from several sequences and merge them into the one stream
- **CombineLatest()** lets you provide a function to combining latest values from each of the selected streams
- **Zip** bundles pairwise values from 2 sequences
- If you want to zip > 2 sequences, use **And-Then-When**
- **Buffer(N)** bundles N messages at a time into IList<T>
  - Can also have a sliding window or **skip** messages
- **Delay()** delays the entire sequence by the time amount
- **Sample()** - performs samples from a sequence at a specified frequency. **TODO: understand it**
- **Timeout()** generates an error if there is no message in a provided time period
- **Throttle()** - just like Sample() but the wait window is reset whenever a message comes in. 

## Course Summary [32.]

- Rx relies on observable sequences implementing IObservable
- Consumers implement IObserver
  - OnNext -> (OnCompleted | OnError)
- Subjects take care of much boilerplate code
- Observable factory methods let us generate sequences
- Operators are similar to LINQ, but:
  - Time-related processing
  - Processing of interactions between multiple streams
  - Some Rx operations backported to LINQ (Ix.NET)
