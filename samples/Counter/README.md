# Counter
This is a example project demostrating how to share states across RabbitRPC service replicas.

## Defining Service Contract
We start by define a service contract named `ICounterService` in `Shared` project, as done in [HelloWorld](../HelloWorld/README.md) example.
```csharp
[RabbitService(Name = "CounterService")]
public interface ICounterService : IRabbitService
{
    [Action(Name = "Increment")]
    Task<long> IncrementAsync(CancellationToken cancellationToken = default);

    [Action(Name = "Decrement")]
    Task<long> DecrementAsync(CancellationToken cancellationToken = default);

    [Action(Name = "GetCounter")]
    Task<long> GetCounterAsync(CancellationToken cancellationToken = default);
}
```
This time we use `RabbitServiceAttribute` and `ActionAttribute` to customize the name of the service and actions. This may be helpful when you have multiple overloads of the a method in the service contract.

## Implementing the Service
The idea of the `CounterService` is that the service holds a counter start from 0. When `IncrementAsync` is called, the service increments the counter by 1. Similarily, the service  decrements the counter by 1 when `DecrementAsync`. Clients and inspect current value of the counter by calling `GetCounterAsync` without modifying the counter.

The problem is that **all services hosted by RabbitRPC are stateless**. A service instance is created for each request by default, member variables can't survive beyond the lifetime of a request. Also the service may be hosted in different processes or even different computers, which makes it hard to share states across service replicas.

To address the issue, we need to store the states of the service in an external storage where all replicas can access. The storage can be a file on local computer, a Redis server or a relational database.

RabbitRPC provides an abstraction layer , `IStateContext`, to express state storage as an external dependency, which allow services to share states in a storage-agnostic way.

In the `CounterService` example, we'll be using `IStateContext` to store our counter.

```csharp
class CounterService : RabbitService, ICounterService
{
    [RetryOnConcurrencyError]
    public async Task<long> IncrementAsync(CancellationToken cancellationToken = default)
    {
        var val = await StateContext.GetAsync<long>("counter", cancellationToken);
        var newValue = val.Value + 1;
        await StateContext.PutAsync("counter", newValue, val.Version, cancellationToken);

        return newValue;
    }

    [RetryOnConcurrencyError]
    public async Task<long> DecrementAsync(CancellationToken cancellationToken = default)
    {
        var val = await StateContext.GetAsync<long>("counter", cancellationToken);
        var newValue = val.Value - 1;
        await StateContext.PutAsync("counter", newValue, val.Version, cancellationToken);

        return newValue;
    }

    public async Task<long> GetCounterAsync(CancellationToken cancellationToken = default)
    {
        var val = await StateContext.GetAsync<long>("counter", cancellationToken);
        return val.Value;
    }
}
```

There're a few things happening here.

`CounterService` inherits `RabbitService`, this is optional. `RabbitService` provides a few useful properpties such as `CallContext` and `StateContext`, and can also act as a request filter. By inheriting it you can avoid getting those dependencies from your constructor and intercept action excution by simply overriding a filter method. Here in `CounterService` we inherit `RabbitService` to access `StateContext`.

You may also noticed that `IncrementAsync` and `DecrementAsync` are decorated with `RetryOnConcurrencyError`. `IStateContext` supports optimistic concurrency control, when you call `PutAsync` or `DeleteAsync` overloads with `version` parameter, concurrency check will be perform and if the check fails, an `ConcurrencyException` will be thrown. `RetryOnConcurrencyError` checks whether a `ConcurrencyException` is thrown during the action execution, and it re-executes the action if we encountered a concurrency error.

RabbitRPC uses a in-memory `IStateContext` by default, which won't be able to share states across replicas. Here we configure RabbitRPC to use SQLite to store states so that relicas running on the same comuputer can share states:

```csharp
services.AddEntityFrameworkCoreStateContext(options => options.UseSqlite("Data Source=states.db"));
```

## Calling the Service
Calling the `CounterService` is much simpler. For demostration purpose, we increment or decrement the counter by 100 by sending 100 concurrent requests from client.
```csharp
await Task.WhenAll(
    Enumerable.Range(0, 100).Select(_ => _counterService.IncrementAsync(cancellationToken))
);
```
## Running the Example
Now we start least 2 clients and 2 services replicas, then try to increment in one client and decrement in the other. See how the services are handling concurrency errors and check whether the value of counter is consistent after all requests are completed.