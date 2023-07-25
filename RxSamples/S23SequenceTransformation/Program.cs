using Shared;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace S23SequenceTransformation;

internal class Program
{
    static void Main(string[] args)
    {
        var numbers = Observable.Range(1, 10);
        numbers.Select(x => x * x).Inspect("select");

        var subj = new Subject<object>();
        subj.OfType<float>().Inspect("OfTypeFloat");
        // filter and get all floats
        subj.Cast<float>().Inspect("CastFloat");
        // try to cast all to float and throw an exception if cast is impossible
        subj.OnNext(1.0f, 2, 3.0);

        var seq = Observable.Interval(TimeSpan.FromSeconds(1));
        //seq.Timestamp().Inspect("Timestamp");
        //Timestamp has generated value 0@7/25 / 2023 5:17:20 PM +00:00
        //Timestamp has generated value 1@7/25 / 2023 5:17:21 PM +00:00
        //Timestamp has generated value 2@7/25 / 2023 5:17:22 PM +00:00
        //...
        // Current timestamp

        //seq.TimeInterval().Inspect("TimeInterval");
        //TimeInterval has generated value 0@00:00:01.0262434
        //TimeInterval has generated value 1@00:00:01.0049405
        //...
        //This value shows us how much time was elapsed after the previous event was generated (in average = 1 sec)

        Console.ReadLine();
    }
}