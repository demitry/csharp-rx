using Shared;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace S23CollapsingSequences;

internal class Program
{
    static void Main(string[] args)
    {
        // 1  1 2  1 2 3  1 2 3 4
        //var seq = Observable.Range(1, 4)
        //    .SelectMany(x => Observable.Range(1, x))
        //    .Inspect("SelectMany");
        /*
        SelectMany has generated value 1
        SelectMany has generated value 1
        SelectMany has generated value 2
        SelectMany has generated value 1
        SelectMany has generated value 2
        SelectMany has generated value 1
        SelectMany has generated value 3
        SelectMany has generated value 2
        SelectMany has generated value 3
        SelectMany has generated value 4
        SelectMany has completed
        */

        // You cannot expect that everything will be exact in order
        // 

        // You can specify 
        //var seq2 = Observable.Range(1, 4, 
        //    scheduler: Scheduler.Immediate)
        //.SelectMany(x => Observable.Range(1, x))
        //.Inspect("SelectManySchedulerImmediate");

        /*
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 2
        SelectManySchedulerImmediate has generated value 2
        SelectManySchedulerImmediate has generated value 2
        SelectManySchedulerImmediate has generated value 3
        SelectManySchedulerImmediate has generated value 3
        SelectManySchedulerImmediate has generated value 4
        SelectManySchedulerImmediate has completed
         */

        var seq3 = 
            Observable.Range(1, 4, scheduler: Scheduler.Immediate)
            .SelectMany(x => Observable.Range(1, x, scheduler: Scheduler.Immediate))
            .Inspect("SelectManySchedulerImmediate");

        /*
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 2
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 2
        SelectManySchedulerImmediate has generated value 3
        SelectManySchedulerImmediate has generated value 1
        SelectManySchedulerImmediate has generated value 2
        SelectManySchedulerImmediate has generated value 3
        SelectManySchedulerImmediate has generated value 4
        SelectManySchedulerImmediate has completed 
        */
    }
}