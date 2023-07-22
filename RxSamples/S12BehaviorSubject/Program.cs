using System.Reactive.Subjects;
using Shared;

namespace S12BehaviorSubject;

// you want expose the sensor
public class Scada
{
    private BehaviorSubject<double> sensorValue;

    public IObservable<double> SensorValue => sensorValue;  
}

internal class Program
{
    static void Main(string[] args)
    {
        var sensorReading = new BehaviorSubject<double>(-1.0);
        //Initializes a new instance of the BehaviorSubject<T> class which creates a subject that caches its last value and starts with the specified value.

        sensorReading.Inspect("sensor");
        
        sensorReading.OnNext(0.99);
        
        sensorReading.OnCompleted();

//sensor has generated value -1
//sensor has generated value 0.99
//sensor has completed

    }
}