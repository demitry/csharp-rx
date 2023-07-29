using Shared;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace S27ExceptionHandling;

internal class Program
{
    static void Main(string[] args)
    {
        //TestCatchWithFallback();
        //TestCatchWithFallbackEmpty();
        //TestCatchDifferentEx();
        //TestOnErrorResumeNext();
        TestRetry();
    }

    static void TestCatchWithFallback()
    {
        var subject = new Subject<int>();
        var fallback = Observable.Range(1, 3);

        subject
            .Catch(fallback)
            .Inspect("subject");

        subject.OnNext(32);
        subject.OnError(new Exception("Ooops"));

        //subject has generated value 32
        //subject has generated value 1
        //subject has generated value 2
        //subject has generated value 3
        //subject has completed
    }

    static void TestCatchWithFallbackEmpty()
    {
        var subject = new Subject<int>();

        subject
            .Catch(Observable.Empty<int>())
            .Inspect("subject");

        subject.OnNext(32);
        subject.OnError(new Exception("Ooops"));

        //subject has generated value 32
        //subject has completed
    }

    static void TestCatchDifferentEx()
    {
        var subject = new Subject<int>();

        subject
            .Catch<int, ArgumentException>(
              ex => Observable.Return(111)
            )
            .Catch(Observable.Empty<int>())
            .Inspect("subject");

        subject.OnNext(32);
        subject.OnError(new ArgumentException("arg"));
        subject.OnError(new Exception("Ooops"));

        //subject has generated value 32
        //subject has completed
    }

    static void TestOnErrorResumeNext()
    {
        var seq1 = new Subject<char>();
        var seq2 = new Subject<char>();

        seq1.OnErrorResumeNext(seq2).Inspect("OnErrorResumeNext");
        // We don't care, just continue with other sequence

        //abcdef

        seq1.OnNext('a', 'b', 'c')
            .OnError(new Exception());

        seq2.OnNext('d', 'e', 'f')
            .OnCompleted();
    }


    private static IObservable<int> SucceedAfter(int attempts)
    {
        int count = 0;
        return Observable.Create<int>(o =>
        {
            Console.WriteLine((count > 0 ? "Ret" : "T") + "rying to do work");
            if(count++ < attempts)
            {
                Console.WriteLine("Failed");
                o.OnError(new Exception());
            }
            else 
            { 
                Console.WriteLine("Succeeded");
                o.OnNext(count);
                o.OnCompleted();
            }

            return Disposable.Empty;
        });
    }


    static void TestRetry()
    {
        SucceedAfter(3).Retry(4).Inspect("Retry");

        /*
        Trying to do work
        Failed
        Retrying to do work
        Failed
        Retrying to do work
        Failed
        Retrying to do work
        Succeeded
        Retry has generated value 4
        Retry has completed
        */
    }
}