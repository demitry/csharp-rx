# Reactive Extensions for .NET Developers

<https://learn.microsoft.com/en-us/shows/on-net/reactive-extensions-for-net-developers>

UI listens, Click - Do,

Async - one time
Rx - multiple times

Stream of events

## IObservable< T >

- Pushes data to subscribers
- Like conveyor belt or a stream
- May or may not have an end
- Values can be processed with LINQ

## Subscribing

- Gets values from the producer
- Gets values from IObservable<T>
- Can have multiple subscriptions
- Unsubscribe using IDisposable

## Hot and Cold observables

From: Anton Moiseev's Book “Angular Development with Typescript, Second Edition.” :
Hot and cold observables

There are two types of observables: hot and cold. The main difference is that a cold observable creates a data producer for each subscriber, whereas a hot observable creates a data producer first, and each subscriber gets the data from one producer, starting from the moment of subscription.

Let’s compare watching a movie on Netflix to going into a movie theater. Think of yourself as an observer. Anyone who decides to watch Mission: Impossible on Netflix will get the entire movie, regardless of when they hit the play button. Netflix creates a new producer to stream a movie just for you. This is a cold observable.

If you go to a movie theater and the showtime is 4 p.m., the producer is created at 4 p.m., and the streaming begins. If some people (subscribers) are late to the show, they miss the beginning of the movie and can only watch it starting from the moment of arrival. This is a hot observable.

A cold observable starts producing data when some code invokes a subscribe() function on it. For example, your app may declare an observable providing a URL on the server to get certain products. The request will be made only when you subscribe to it. If another script makes the same request to the server, it’ll get the same set of data.

A hot observable produces data even if no subscribers are interested in the data. For example, an accelerometer in your smartphone produces data about the position of your device, even if no app subscribes to this data. A server can produce the latest stock prices even if no user is interested in this stock.

- <https://learn.microsoft.com/en-us/shows/on-net/reactive-extensions-for-net-developers>
- <https://learn.microsoft.com/en-us/previous-versions/dotnet/reactive-extensions/hh242985(v=vs.103)?redirectedfrom=MSDN>
- <https://github.com/TheEightBot/Reactive-Examples>
- <https://github.com/dotnet/reactive>
- <https://rxmarbles.com/>
- <http://introtorx.com/>
- <https://reactivex.io/>
- <https://habr.com/ru/articles/269417/>
- <https://habr.com/ru/articles/270023/>
