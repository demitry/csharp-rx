using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Shared;

namespace S20ConvertingIntoObservables;

internal class Program
{
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

    static void Main(string[] args)
    {
        //TestObservableStart();
        //TestFromEventPattern();
        TestTaskToObservable();
        TestIEnumerablesToObservable();
    }

    static void TestIEnumerablesToObservable()
    {
        var items = new List<int> { 1, 2, 3 };
        var source = items.ToObservable();
        source.Inspect("ItemsToObservable");
    }

    static void TestTaskToObservable()
    {
        var task = Task.Factory.StartNew(() => "Test");
        var source = task.ToObservable();
        source.Inspect("task");
        Console.ReadLine();
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