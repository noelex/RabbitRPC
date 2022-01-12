# RemoteTimer
This is a example project demostrating how to use event bus in RabbitRPC.

## Defining the Event
RabbitRPC has no requirement on the definition of an event type. We define our `TimerEvent` in `Shared` project:
```csharp
public class TimerEvent
{
    public long Id { get; set; }
}
```

## Publishing the Event
Event bus is not tied to RabbitRPC service host or client. It can be used as a standalone component by registering it during application startup:
```csharp
services.AddMessagePackSerializationProvider();
services.AddRabbitMQConnectionProvider("amqp://guest:guest@localhost/");
services.AddEventBus();
```
`AddRabbitServiceHost` and `AddRabbitServiceClient` calls `AddEventBus` internally, if you're using event bus with RabbitRPC service host or client, there's no need to call `services.AddEventBus()`.

We'll be publishing `TimerEvent` each second form `EventPublisher`. The code is pretty straigh forward:
```csharp
private async Task RunAsync(CancellationToken cancellationToken)
{
    var eventId = 0L;
    while (!cancellationToken.IsCancellationRequested)
    {
        await Task.Delay(1000, cancellationToken);

        _eventBus.Publish(new TimerEvent { Id = eventId++ });
    }
}
```

## Consuming the Event
We subsribe the `TimerEvent` in `EventSubscriber` and prints the event ID when an event is received:
```csharp
_eventBus.Observe<TimerEvent>()
         .Subscribe(e => Console.WriteLine($"Received TimerEvent {{Id={e.Id}}}"), _cancellationTokenSource.Token);
```

## Running the Example
Now we can start an `EventPublisher` and multiple `EventSubscriber`s. See whether the events are broadcasted to all subscribers.