namespace S24SequenceAggregation;
using Shared;
using System.Reactive.Linq;
using System.Reactive.Subjects;

//
internal class Program
{
    static void Main(string[] args)
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