using JetBrains.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace S03ObserverDesignPattern;

public class Market : INotifyPropertyChanged
{
    private float volatility;

    public float Volatility
    {
        get => volatility;
        set
        {
            if(value.Equals(volatility)) return;
            volatility = value;
            OnPropertyChanged(nameof(Volatility));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = default)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

internal class Program
{
    static void Main(string[] args)
    {
        var market = new Market();
        market.PropertyChanged += (sender, eventArgs) =>
        {
            if (eventArgs.PropertyName == "Volatility")
            {
                Console.WriteLine("Wow! Volatility was changed");
            }
        };

        market.Volatility = 2.3F;
    }
}