namespace Shared;

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
