using Shared;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace S21SequenceFiltering;

internal class Program
{
    static void Main(string[] args)
    {
        Observable.Range(0, 100)
            .Where(i => i % 9 == 0)
            .Inspect("where");

        var values = Observable.Range(-10, 21);
        values
            .Select(i => i * i)
            .Distinct()
            .Inspect("select distinct");

        new List<int> { 1, 1, 2, 2, 3, 3, 2, 2 }
        .ToObservable()
        .DistinctUntilChanged() //looks on previous value
        //.IgnoreElements()
        .Inspect("DistinctUntilChanged");

        Observable.Range(1, 10)
            .Skip(3)
            .Take(4)
            .Inspect("Skip3Take4");

        Observable.Range(-10, 21)
            .SkipWhile(x => x < 0)
            .TakeWhile(x => x < 6)
            .Inspect("SkipWhileTakeWhile");

        Observable.Range(-10, 21)
            .SkipLast(5) 
            .Inspect("SkipLast");
        // problematic because you have to know how many elements do you have

        //values.SkipUntil()

        var stockPrices = new Subject<float>();
        var optionPrices = new Subject<float>();

        stockPrices.SkipUntil(optionPrices)
            .Inspect("skipuntil");

        stockPrices.OnNext(1);
        stockPrices.OnNext(2);
        stockPrices.OnNext(3);
        optionPrices.OnNext(55);
        stockPrices.OnNext(4, 5, 6); // only these will be available
        //skipuntil has generated value 4
        //skipuntil has generated value 5
        //skipuntil has generated value 6
    }
}