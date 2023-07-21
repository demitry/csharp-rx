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
    - [Unsubscribing [9.]](#unsubscribing-9)
    - [Proxy and Broadcast [10.]](#proxy-and-broadcast-10)
    - [ReSubject [11.]](#resubject-11)
    - [BehaviorSubject [12.]](#behaviorsubject-12)
    - [AsyncSubject [13.]](#asyncsubject-13)
    - [Implementing IObservable [14.]](#implementing-iobservable-14)
    - [Quiz 2: Subjects [  ]](#quiz-2-subjects---)
    - [Summary [15.]](#summary-15)
    - [Overview [16.]](#overview-16)
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

## Subject [8.]

## Unsubscribing [9.]

## Proxy and Broadcast [10.]

## ReSubject [11.]

## BehaviorSubject [12.]

## AsyncSubject [13.]

## Implementing IObservable [14.]

## Quiz 2: Subjects [  ]

## Summary [15.]

## Overview [16.]

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

