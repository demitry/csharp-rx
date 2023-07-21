using System.ComponentModel;

namespace S03ObserverDesignPattern3;

public class Market // observable
{
    public BindingList<float> Prices = new BindingList<float>();

    public void AddPrice(float price)
    {
        Prices.Add(price);
    }
}

internal class Program // observer
{
    static void Main(string[] args)
    {
        var market = new Market();

        market.Prices.ListChanged += (sender, eventArgs) =>
        {
            if (eventArgs.ListChangedType == ListChangedType.ItemAdded)
            {
                if (sender != null)
                {
                    float price = ((BindingList<float>)sender)[eventArgs.NewIndex];
                    Console.WriteLine($"Binding list got a price of {price}");
                }
            }
        };

        market.AddPrice(123);
    }
}

// Problem:
// But what if market crashed, or no new prices added?
// Create MarkedCrashed events?