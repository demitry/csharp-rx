namespace S03ObserverDesignPattern2;

public class Market
{
    private List<float> prices = new List<float>();

    public void AddPrice(float price)
    {
        prices.Add(price);
        // Classic:
        PriceAdded?.Invoke(sender: this, price);
    }

    // Classic:
    public event EventHandler<float> PriceAdded;
}

internal class Program
{
    static void Main(string[] args)
    {
        var market = new Market();
        
        // Subscribe:
        market.PriceAdded += (sender, f) =>
        {
            Console.WriteLine($"We got a price of {f}");
        };

        market.AddPrice(100);
    }
}