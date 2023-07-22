using System.Reactive.Subjects;

namespace S09Unsubscribing;

/*
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
*/

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