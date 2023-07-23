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
