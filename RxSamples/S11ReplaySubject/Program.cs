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