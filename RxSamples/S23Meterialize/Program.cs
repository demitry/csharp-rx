using Shared;
using System.Reactive.Linq;

namespace S23Meterialize;

internal class Program
{
    static void Main(string[] args)
    {
        // When we are talking about materialization/dematerialization
        // in LINQ terminology, when materialize a IEnumerable -
        // what you really do - a converting to a List or another collection
        // In Reactive Extensions materialization/dematerialization is 
        // ABSOLUTEKY DIFFERENT THING:
        // You generate some kind of debugging information for a particular sequence:

        var seq = Observable.Range(0, 4);
        seq.Materialize().Inspect("Materialize");
        Console.ReadLine();

        //Materialize has generated value OnNext(0)
        //Materialize has generated value OnNext(1)
        //Materialize has generated value OnNext(2)
        //Materialize has generated value OnNext(3)
        //Materialize has generated value OnCompleted()
        //Materialize has completed

        // Instead of giving us values 0,1,2,3 it gives
        // "value" OnNext(), OnCompleted()

        // public static IObservable<Notification<TSource>> Materialize<TSource>(this IObservable<TSource> source)
        //public static IObservable<Notification<int>> Materialize<int>(this IObservable<int> source)

        /*
        Notification<int> notification;
        switch (notification.Kind)
        {
            case NotificationKind.OnCompleted: break;
            case NotificationKind.OnError: break;
            case NotificationKind.OnNext: break;
        }
        notification.Value
        */

        //Dematerialize() = 
        // gets you back from notifications of int to ordinary observable sequence

        var seq2 = Observable.Range(0, 4);
        seq2.Materialize()
            .Dematerialize()
            .Inspect("Dematerialize");
        Console.ReadLine();

        /*
        Materialize has generated value OnNext(0)
        Materialize has generated value OnNext(1)
        Materialize has generated value OnNext(2)
        Materialize has generated value OnNext(3)
        Materialize has generated value OnCompleted()
        Materialize has completed
        b
        Dematerialize has generated value 0
        Dematerialize has generated value 1
        Dematerialize has generated value 2
        Dematerialize has generated value 3
        Dematerialize has completed
        */
    }
}