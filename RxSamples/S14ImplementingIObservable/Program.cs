using System.Collections.Immutable;
using System.Reactive.Disposables;
using Shared;

namespace S14ImplementingIObservable
{
    public class Market : IObservable<float>
    {
        // Keep a list of all components which are observing you ?
        // What do you use? List?
        // HashSet to prevent duplicates?
        // ConcurrentBag for Thread-Safety?

        private ImmutableHashSet<IObserver<float>> observers =
            ImmutableHashSet<IObserver<float>>.Empty;

        //public class Subscription : IDisposable
        //{
        //    public void Dispose()
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        public IDisposable Subscribe(IObserver<float> observer)
        {
            observers = observers.Add(observer);
            return Disposable.Create(
                dispose: () => {
                    observers = observers.Remove(observer);
                });
        }

        public void Publish(float price)
        {
            foreach(var o in observers)
            {
                o.OnNext(price);
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var market = new Market();
            var subscription = market.Inspect("market");

            market.Publish(123.0f); // market has generated value 123

            subscription.Dispose();

            market.Publish(321.0f); // no, already disposed
        }
    }
}