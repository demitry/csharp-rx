using System.Reactive.Subjects;

namespace S07Subjects;

/*
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
*/


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