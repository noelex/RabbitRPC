# RabbitRPC
> This repository is a work-in-progress.

RabbitRPC is a lightweight .NET communication library for microservices and distributed applications built on top of RabbitMQ.

RabbitRPC aims to simplify the development and deployment of small and mid-sized distributed applications by providing zero configuration communication primitives including remote procedure call, distributed events, shared states and distributed work queues.

RabbitRPC doesn't rely on any microservice infrastructure or development tool. But you can have a smoother development/deployment experience by using [Project Tye](https://github.com/dotnet/tye).
# Features

## Strongly-typed RPC services and clients
RabbitRPC allows you to setup RPC services inside your ASP.NET Core or standalone console applications with just a few lines of code, zero configuration.
```csharp

public interface IChatService　:　IRabbitService
{
    Task<string> HelloAsync(string name, CancellationToken cancellationToken = default);
}

class ChatService : IChatService
{
    public Task<string> HelloAsync(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Hello {name}!");
    }
}
```
Then you can access the service with a strongly-typed client:
```csharp
class ClientApp
{
    private readonly IChatService _chatService;

    public ClientApp(IChatService service)
    {
        _chatService = service;
    }

    publice async Task SayHelloAsync(CancellationToken cancellationToken)
    {
        var greeting = await _chatService.HelloAsync("World", cancellationToken);
        Console.WriteLine(greeting);
    }
}
```
No need to worry about service discovery, transport, serialization and contracts. RabbitRPC will handle that for you.

## Automatically load-balanced
All RabbitRPC services are by default load-balanced, no configuration is needed. You can run as many replicas as you want, on one computer or across the network. Requests to the service will be evenly distributed to all replicas in a round-robin manner.

## Request filtering
RabbitRPC provides a ASP.NET Core MVC like request filtering pipeline, custom cross-cutting concerns can be added to your service actions by defining your own filters.

Currently the following hooks are supported:

- `OnInitializeServiceInstance`: Prepare service instance to process the request.
- `OnBindParameters`: Read parameters from raw request and bind them to action method parameters.
- `OnActionExecuting`: Called before the action exectuion.
- `OnActionExecuted`: Called after the action execution.

Client proxies also supports request filtering. You can intercept, inspect and modify request and response messages before they are sent or processed.

Client filters provides the following hook timings:

- `OnPrepareRequest`: Convert method parameters to request message and add custom out-of-band data by using RabbitMQ message properties.
- `OnRequestStarting`: Called before the request is sent to the server.
- `OnResponseReceived`: Called after the response is received from the server.
- `OnRequestCompleted`: Called when the result or error information are extracted from the response.

## Shared states
Sometimes you may want to have shared states across multiple replicas. You can achieve this by using a third-party state storage like Redis or a relational database.

RabbitRPC also provides a simple state storage interface which allows you to access different state storage providers. The interface supports transactions and optimistic concurrency control.

To access shared states inside your RPC serivce, simply use `StateContext` property:
```csharp
await StateContext.GetAsync<long>("counter", cancellationToken);
```

You can also leverage the `RetryOnConcurrencyErrorAttribute` action filter to perform concurrency control. The filter will detect any concurrency error occurred during the action execution, and will re-execute the action to retry automatically.

RabbitRPC uses a in-memory provider by default. With the in-memory provider, states cannot be shared among replicas. To use a different provider, you need to register the state context on startup with:
```csharp
services.AddEntityFrameworkCoreStateContext(options => options.UseSqlite("Data Source=states.db"));
```

## Distributed events
RabbitRPC provides a simple yet powerful strongly-typed event bus, which allows you to publish and subcribe distributed events with low coding overhead.

To publish an event, simply call `IRabbitEventBus.Publish` and the event will be sent to all subscribers.
```csharp
EventBus.Publish(new MyEvent());
```
To subscribe a event, call `IRabbitEventBus.Observe<T>` to retrieve an `IObservable<T>` instance for the specific type of event. And you can subscribe the event by calling `IObservable<T>.Subscribe` to receive strongly-typed event objects.
```csharp
EventBus.Observe<MyEvent>().Subscribe(...);
```
Since the events are streamed through `IObservable<T>`, you can also use [Reactive Extensions](https://github.com/dotnet/reactive) to manipulate and consume the event stream.

## Work queues
RabbitRPC implements a distributed durable work queue to support background batch processing.
You can run the work item handler along with the RPC service, or in a seperate application.

To utilize background batch processing, you need to define a handler:
```csharp
class PrintJobHandler : IWorkItemHandler<PrintJob>
{
    public async Task ProcessAsync(ReadOnlyMemory<WorkItem<PrintJob>> items, CancellationToken cancellationToken)
    {
        for(var i=0; i<items.Length; i++)
        {
            Console.WriteLine(items.Span[i].Value.Text);
            items.Span[i].IsDone = true;
        }
    }
}
```
Then register this handler with work queue:
```csharp
services.AddWorkQueue(x=>x.AddHandler<PrintJob, PrintJobHandler>(options=>
{
    options.ConcurrencyMode = BatchConcurrencyMode.Shared;
    options.BatchTimeout = 1000;
    options.BatchSize = 8;
}));
```
Once the handler is up and running, you can dispatch works by a single line of code:
```csharp
_workQueue.Post(new PrintJob($"Hello world!"));
```
Similar to RPC services, work item handlers also support load balancing by default. You can have multiple work item handler which handles same work item type running in different processes or computers.

# Getting Started
RabbitRPC is currently under development, you can install preview packages to try out:
```
dotnet add package RabbitRPC.Core --prerelease
```
You'll also need to install a serialization provider package using either
```
dotnet add package RabbitRPC.Serialization.NewtonsoftJson --prerelease
```
or
```
dotnet add package RabbitRPC.Serialization.MessagePack --prerelease
```
To share states between replicas, you can install a EntityFrameworkCore state storage provider which supports using SQLite, Microsoft SQL Server and PostgreSQL to store shared states:
```
dotnet add package RabbitRPC.States.EntityFrameworkCore --prerelease
```
Please refer `samples` diretory for detailed usage information.
To run the applications in `samples`, you'll need a working RabbitMQ instance.
