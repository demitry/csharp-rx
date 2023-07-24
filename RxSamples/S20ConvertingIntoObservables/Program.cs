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