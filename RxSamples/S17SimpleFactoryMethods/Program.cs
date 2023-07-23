using Shared;
using System.Reactive.Linq;

namespace S17SimpleFactoryMethods;

internal class Program
{
    static void Main(string[] args)
    {
        //var obs = Observable.Return<int>(42);
        var obs = Observable.Return(42); // ReplaySubject
        obs.Inspect("obs"); // obs has generated value 42
                            // obs has completed

        var obsEmpty = Observable.Empty<int>(); 
        // doesn't produce the value but produces completion signal
        obsEmpty.Inspect("obsEmpty"); // obsEmpty has completed

        var obsNever = Observable.Never<int>(); // no value, no signal
        obsNever.Inspect("obsNever");

        var obsThrow = Observable.Throw<int>(new Exception("oops"));
        obsThrow.Inspect("obsThrow");
    }
}