namespace S22SequenceInspection;
using Shared;
using System.Reactive.Linq;
using System.Reactive.Subjects;

internal class Program
{
    static void Main(string[] args)
    {
        var subject = new Subject<int>();
        subject.Any(x => x > 1).Inspect("any");
        subject.OnCompleted();
        //any has generated value False
        //any has completed

        //We haven't this at LINQ but have in Reactive Extensions
        // Sequence MUST be completed (subject.OnCompleted(); is called)
        // In the other case we cannot make a decision about these values.

        //subject.OnNext(2);

        var values = new List<int> {/* -1, -2, 0,*/ 1, 2, 3, 4, 5 };
        values.ToObservable()
            .All(x => x > 0)
            .Inspect("all"); 
        //all has generated value True
        //all has completed

        var subj = new Subject<string>();
        subj.Contains("foo").Inspect("contains");
        subj.OnNext("foo");
        //contains has generated value True
        //contains has completed

        // I want to get notifications from this observable
        // but if it has no values, please give me some kind of default
        // DefaultIfEmpty() provides you default value if sequence generated no values
        // but the sequence must be completed
        var subjB = new Subject<float>();
        subjB.DefaultIfEmpty(0.99f)
            .Inspect("subjB");
        //subjB.OnCompleted(); //subjB has generated value 0.99, subjB has completed
        //or
        subjB.OnNext(1.2f); //subjB has generated value 1.2

        var numbers = Observable.Range(0, 10);
        numbers.ElementAt(5)
            .Inspect("ElementAt");
        //ElementAt has generated value 5
        //ElementAt has completed

        var numbers2 = Observable.Range(0, 10);
        numbers2.ElementAt(15)
            .Inspect("ElementAt15");
        // ElementAt15 has generated exception Specified argument was out of the range of valid values. (Parameter 'index')
        //remember Inspect():
        //ex => Console.WriteLine($"{name} has generated exception {ex.Message}")

        var seq1 = new Subject<int>();
        var seq2 = new Subject<int>();

        seq1.Inspect("seq1");
        seq2.Inspect("seq2");

        seq1.SequenceEqual(seq2)
            .Inspect("SequenceEqual");

        //seq1.OnNext(1);
        //seq2.OnNext(2);
        //SequenceEqual has generated value False
        //SequenceEqual has completed

        seq1.OnNext(1);
        seq2.OnNext(1);
        seq1.OnCompleted();
        seq2.OnCompleted();
        //seq1 has generated value 1
        //seq2 has generated value 1
        //seq1 has completed
        //seq2 has completed
        //SequenceEqual has generated value True
        //SequenceEqual has completed
    }
}