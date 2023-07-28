namespace S24SequenceAggregation;
using Shared;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Xml.Linq;

//
internal class Program
{
    static void Main(string[] args)
    {
        //TestCountMinMaxAverage();

        TestFirstAsyncFirstOrDefaultAsync();

    }

    static void TestFirstAsyncFirstOrDefaultAsync()
    {
        var replay = new ReplaySubject<int>();

        replay.OnNext(-1);
        replay.OnNext(2);
        replay.OnCompleted(); // it is replay subj so san subscribe after

        replay.FirstAsync(i => i > 0) // First() is deprecated
            .Inspect("FirstAsync");

        //FirstAsync has generated value 2
        //FirstAsync has completed

        //return Observable that yields the actual value
        //public static IObservable<TSource> FirstAsync<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)

        // What is we have no value > 0 ?
        var replay2 = new ReplaySubject<int>();
        replay2.OnNext(-1);
        replay2.OnCompleted();

        //replay2.FirstAsync(i => i > 0).Inspect("FirstAsync2");

        //FirstAsync2 has generated exception Sequence contains no matching element.
        // The same behavior in LINQ

        replay2.FirstOrDefaultAsync(i => i > 0).Inspect("FirstAsync2");
        //Default for int is 0, so
        //FirstAsync2 has generated value 0
        //FirstAsync2 has completed
    }

    static void TestCountMinMaxAverage()
    {
        // Collapsing reactive sequence
        var values = Observable.Range(1, 5);
        values.Inspect("Values");
        values.Count().Inspect("count");
        /*
        Values has generated value 1
        Values has generated value 2
        Values has generated value 3
        Values has generated value 4
        Values has generated value 5
        Values has completed
        count has generated value 5
        count has completed
        */

        var intSubj = new Subject<int>();
        intSubj.Inspect("intSubj");
        intSubj.Min().Inspect("min");
        intSubj.Max().Inspect("max");
        intSubj.Average().Inspect("avg"); // Average() returns double

        intSubj.OnNext(2);
        intSubj.OnNext(4);
        intSubj.OnNext(5);
        intSubj.OnNext(12);

        intSubj.OnCompleted(); // NB!

        /*
            intSubj has generated value 2
            intSubj has generated value 4
            intSubj has generated value 5
            intSubj has generated value 12
            intSubj has completed
            min has generated value 2
            min has completed
            max has generated value 12
            max has completed
            avg has generated value 5.75
            avg has completed  
         */
    }
}