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