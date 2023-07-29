using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Shared;

namespace S28SequenceCombinators;

//CombineLatest, Zip, And-Then-When (Zip Multiple Sequences), Concat, Repeat, StartWith, Amb(iguous), Merge

internal class Program
{
    static void Main(string[] args)
    {
        TestCombineLatest();

        TestZip();

        TestZipThreeSequences();

        TestConcatRepeatStartWith();

        TestAmbiguous();

        TestMerge();
    }

    static void TestCombineLatest()
    {
        var mechanical = new BehaviorSubject<bool>(true);
        var electrical = new BehaviorSubject<bool>(true);
        var electronic = new BehaviorSubject<bool>(true);

        mechanical.Inspect("mechanical");
        electrical.Inspect("electrical");
        electronic.Inspect("electronic");

        // True
        // True, what if False?
        // True

        Observable.CombineLatest(mechanical, electrical, electronic)
          .Select(values => values.All(x => x))
          .Inspect("Is the system OK?");

        // True

        mechanical.OnNext(false); //break system

        // => False
    }

    static void TestZip()
    {
        var digits = Observable.Range(0, 10);
        var letters = Observable.Range(0, 10).Select(x => (char)('A' + x));

        letters
            .Zip(digits, (letter, digit) => $"{letter}-{digit}")
            .Inspect("Zip");

        //Zip has generated value A-0
        //Zip has generated value B-1
        //Zip has generated value C-2
        //Zip has generated value D-3
        //Zip has generated value E-4
        //Zip has generated value F-5
        //Zip has generated value G-6
        //Zip has generated value H-7
        //Zip has generated value I-8
        //Zip has generated value J-9
        //Zip has completed
    }

    static void TestZipThreeSequences()
    {
        var digits = Observable.Range(0, 10);
        var letters = Observable.Range(0, 10).Select(x => (char)('A' + x));
        // And returns Pattern<T1, T2> whose properties are internal
        var punctuation = "!@#$%^&*()".ToCharArray().ToObservable();

        Observable.When( // return type is Plan<> and all that
            digits
              .And(letters)
              .And(punctuation)
              .Then((digit, letter, symbol) => $"{digit}-{letter}-{symbol}")
              )
            .Inspect("And-Then-When");
        /*
            And-Then-When has generated value 0-A-!
            And-Then-When has generated value 1-B-@
            And-Then-When has generated value 2-C-#
            And-Then-When has generated value 3-D-$
            And-Then-When has generated value 4-E-%
            And-Then-When has generated value 5-F-^
            And-Then-When has generated value 6-G-&
            And-Then-When has generated value 7-H-*
            And-Then-When has generated value 8-I-(
            And-Then-When has generated value 9-J-)
            And-Then-When has completed
         */
    }

    static void TestConcatRepeatStartWith()
    {
        // Concat merges all the sequences into one
        var s1 = Observable.Range(1, 3); // 1,2,3
        var s2 = Observable.Range(4, 3); // 4,5,6
        s2.Concat(s1).Inspect("Concat");
        //Concat has generated value 4
        //Concat has generated value 5
        //Concat has generated value 6
        //Concat has generated value 1
        //Concat has generated value 2
        //Concat has generated value 3
        //Concat has completed

        // Repeat repeats the sequence as often as is necessary
        s1.Repeat(3).Inspect("Repeat");
        //Repeat has generated value 1
        //Repeat has generated value 2
        //Repeat has generated value 3
        //Repeat has generated value 1
        //Repeat has generated value 2
        //Repeat has generated value 3
        //Repeat has generated value 1
        //Repeat has generated value 2
        //Repeat has generated value 3
        //Repeat has completed

        s1.StartWith(2, 1, 0).Inspect("StartWith"); // Adds values before 
        //StartWith has generated value 2
        //StartWith has generated value 1
        //StartWith has generated value 0
        //StartWith has generated value 1
        //StartWith has generated value 2
        //StartWith has generated value 3
        //StartWith has completed
    }

    static void TestAmbiguous()
    {
        // Amb(iguous)
        // will return a value from the sequence that first produces a value
        // will ignore values from all other sequences

        var seq1 = new Subject<int>();
        var seq2 = new Subject<int>();
        var seq3 = new Subject<int>();
        seq1.Amb(seq2).Amb(seq3).Inspect("Amb");
        //seq1.OnNext(1); // 1, 10, 100, but comment this out => 2,20,200
        seq2.OnNext(2);
        seq3.OnNext(3);
        seq1.OnNext(10);
        seq2.OnNext(20);
        seq3.OnNext(30);
        seq1.OnNext(100);
        seq2.OnNext(200);
        seq3.OnNext(300);
        seq1.OnCompleted();
        seq2.OnCompleted();
        seq3.OnCompleted();
        //Amb has generated value 1
        //Amb has generated value 10
        //Amb has generated value 100
        //Amb has completed
        // The first seq is used!
    }

    static void TestMerge()
    {
        // Merge pairs up values from multiple sequences
        // Gets info as soon as it comes in
        var foo = new Subject<long>();
        var bar = new Subject<long>();
        var baz = Observable.Interval(TimeSpan.FromMilliseconds(500)).Take(5);

        foo.Merge(bar).Merge(baz).Inspect("Merge");

        foo.OnNext(100);
        Thread.Sleep(1000);
        bar.OnNext(10);
        Thread.Sleep(1000);
        foo.OnNext(1000);
        Thread.Sleep(1000);

        //Merge has generated value 100
        //Merge has generated value 0
        //Merge has generated value 1
        //Merge has generated value 10
        //Merge has generated value 2
        //Merge has generated value 3
        //Merge has generated value 1000
        //Merge has generated value 4
    }
}