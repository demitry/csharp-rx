using System.Reactive.Subjects;
using Shared;

namespace S13AsyncSubject;

internal class Program
{
    static void Main(string[] args)
    {
        //Task<int> task = Task.Run(() => 42);
        //int t = task.Result;
        //similar
        var sensor = new AsyncSubject<double>();
        sensor.Inspect("async");

        sensor.OnNext(1.0);
        sensor.OnNext(2.0);

        // if we run at that point => no output because Completed is not called

        // but if we call:
        sensor.OnCompleted();

        // We will have the last value sent into receiver:
        //Output:
        //async has generated value 2
        //async has completed

        sensor.OnNext(123);//after the OnCompleted call, according to the contract, no exception but it will not do anything
        sensor.OnCompleted();
    }
}