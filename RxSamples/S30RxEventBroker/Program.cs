using Autofac;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace S30RxEventBroker;

internal class Program
{
    public class Actor
    {
        protected EventBroker broker;

        public Actor(EventBroker broker)
        {
            this.broker = broker ?? throw new ArgumentNullException(nameof(broker));
        }
    }

    public class FootballPlayer : Actor
    {
        public string Name { get; set; }

        public int GoalsScored { get; set; } = 0;

        public void Score()
        {
            GoalsScored++;
            broker.Publish(new PlayerScoredEvent { Name = Name, GoalsScored = GoalsScored });
        }

        public void AssaultReferee()
        {
            broker.Publish(new PlayerSentOffEvent { Name = Name, Reason = "violence" });
        }

        public FootballPlayer(EventBroker broker, string name) : base(broker)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            broker.OfType<PlayerScoredEvent>()
                .Where(ps => !ps.Name.Equals(name))
                .Subscribe
                (
                    ps => Console.WriteLine($"{name}: Nicely done, {ps.Name}! It's your {ps.GoalsScored} goal.")
                );

            broker.OfType<PlayerSentOffEvent>()
                .Where(ps => !ps.Name.Equals(name))
                .Subscribe(
                ps => Console.WriteLine($"{name}: See you in the lockers, {ps.Name}!")
                );
        }
    }

    public class FootballCoach : Actor
    {
        public FootballCoach(EventBroker broker) : base(broker)
        {
            broker.OfType<PlayerScoredEvent>()
                .Subscribe(pe =>
                {
                    if (pe.GoalsScored < 3)
                        Console.WriteLine($"Coach: well done, {pe.Name}");
                });

            broker.OfType<PlayerSentOffEvent>()
                .Subscribe(pe =>
                {
                    Console.WriteLine($"Coach: well done, {pe.Name}");

                });
        }
    }

    public class PlayerEvent
    {
        public string Name { get; set; }
    }

    public class PlayerScoredEvent : PlayerEvent
    {
        public int GoalsScored { get; set; }
    }

    public class PlayerSentOffEvent : PlayerEvent
    {
        public string Reason { get; set; }
    }

    public class EventBroker : IObservable<PlayerEvent>
    {
        private Subject<PlayerEvent> subscriptions = new Subject<PlayerEvent>();

        IDisposable IObservable<PlayerEvent>.Subscribe(IObserver<PlayerEvent> observer)
        {
            return subscriptions.Subscribe(observer);
        }

        public void Publish(PlayerEvent pe)
        {
            subscriptions.OnNext(pe);
        }
    }

    static void Main(string[] args)
    {
        // Actor Pattern
        // Football Game
        // EventBroker = glue everything together
        // Propagate EventBroker everywhere with DI
        var cb = new ContainerBuilder();
        cb.RegisterType<EventBroker>().SingleInstance();
        cb.RegisterType<FootballCoach>();
        cb.Register((c, p) =>
            new FootballPlayer(
                c.Resolve<EventBroker>(), 
                p.Named<string>("name")
                ));
        using (var c = cb.Build())
        {
            var coach = c.Resolve<FootballCoach>();
            var player1 = c.Resolve<FootballPlayer>(new NamedParameter("name", "John"));
            var player2 = c.Resolve<FootballPlayer>(new NamedParameter("name", "Chris"));

            player1.Score();
            player1.Score();
            player1.Score(); //x
            player1.AssaultReferee();
            player2.Score();

            //Coach: well done, John
            //Chris: Nicely done, John!It's your 1 goal.
            //Coach: well done, John
            //Chris: Nicely done, John!It's your 2 goal.
            //Chris: Nicely done, John!It's your 3 goal.
            //Coach: well done, John
            //Chris: See you in the lockers, John!
            //Coach: well done, Chris
            //John: Nicely done, Chris!It's your 1 goal. // We can store and dispose the subscription there.
        }
    }
}