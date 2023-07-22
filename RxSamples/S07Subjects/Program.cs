namespace S07Subjects;

public class Market : IObservable<float>
{
    public IDisposable Subscribe(IObserver<float> observer)
    {
        throw new NotImplementedException();
    }
}

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