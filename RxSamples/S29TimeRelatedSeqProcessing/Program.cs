using Shared;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;

namespace S29TimeRelatedSeqProcessing;

internal class Program
{
    static void Main(string[] args)
    {
        //TestBuffer();

        //TestTimestamp();

        //TestDelay();

        //TestSample();

        TestKeyboardInputAndThrottleAndTimeout();
    }

    static void TestBuffer()
    {
        Observable.Range(1, 40)
            .Buffer(5)          //Buffer() - returns Observable<List<int>>
            .Subscribe(x => Console.WriteLine(string.Join(",", x)));

        /*
          1,2,3,4,5
          6,7,8,9,10
          11,12,13,14,15
            ...
          31,32,33,34,35
          36,37,38,39,40
        */
    }

    static void TestTimestamp()
    {
        Observable.Range(1, 40).Buffer(5, 3) // skip 1,2,3; than skip 4,5,6; ...
            .Subscribe(x => Console.WriteLine(string.Join(",", x)));
        /*
            1,2,3,4,5
            4,5,6,7,8
            7,8,9,10,11
            10,11,12,13,14
            ...
            34,35,36,37,38
            37,38,39,40
            40
         */

        var source = Observable.Interval(TimeSpan.FromSeconds(1))
            .Take(3);

        var delay = source.Delay(TimeSpan.FromSeconds(2));
        source.Timestamp().Inspect("source");
        delay.Timestamp().Inspect("delay");
    }

    static void TestDelay()
    {
        var source = Observable.Interval(TimeSpan.FromSeconds(1))
            .Take(3);

        var delay = source.Delay(TimeSpan.FromSeconds(2));
        source.Timestamp().Inspect("source");
        delay.Timestamp().Inspect("delay");

        Console.ReadLine();
        //source has generated value 0@7/30/2023 7:02:20 PM +00:00
        //source has generated value 1@7/30/2023 7:02:21 PM +00:00
        //source has generated value 2@7/30/2023 7:02:22 PM +00:00
        //source has completed
        //delay has generated value 0@7/30/2023 7:02:22 PM +00:00
        //delay has generated value 1@7/30/2023 7:02:23 PM +00:00
        //delay has generated value 2@7/30/2023 7:02:24 PM +00:00
        //delay has completed
    }

    static void TestSample()
    {
        var samples = Observable.Interval(TimeSpan.FromSeconds(0.5))
            .Take(20)
            .Sample(TimeSpan.FromSeconds(1.75));
        samples.Inspect("sample");
        samples.ToTask().Wait();

        //sample has generated value 2
        //sample has generated value 6
        //sample has generated value 9
        //sample has generated value 13
        //sample has generated value 16
        //sample has generated value 19
        //sample has completed
    }

    static void TestKeyboardInputAndThrottleAndTimeout()
    {
        var subj = new Subject<string>();
        subj
            //.Throttle(TimeSpan.FromSeconds(1))
            .Timeout(TimeSpan.FromSeconds(3))
            .Inspect("subj");

        string input = string.Empty;
        ConsoleKeyInfo c;
        while ((c = Console.ReadKey()).Key != ConsoleKey.Escape )
        {
            if(char.IsLetterOrDigit(c.KeyChar) ) 
            {
                input += c.KeyChar;
                subj.OnNext(input);
            }
        }
        //Hsubj has generated value H
        //esubj has generated value He
        //lsubj has generated value Hel
        //lsubj has generated value Hell
        //osubj has generated value Hello
    }
}

//TODO: Review