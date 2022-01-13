# HelloWorld
This is a minimal example demostrating how to use RabbitRPC to perform remote procedure call.

## Defining Service Contract
A service contract named `IChatService` is defined in `Shared` project, which can be resused by server and client.
```csharp
public interface IChatService : IRabbitService
{
    Task<string> HelloAsync(string name, CancellationToken cancellationToken = default);
}
```
There're a few points to note here.

First, the service contract implements `IRabbitService`, which is an empty interface. This is a marker interface used to aid service host to discover service implementation in an assembly.

Second, the action method returns a `Task<string>`. In RabbitRPC, return value of an action must be a `Task` or `Task<T>`. This is because remote procedure calls are by nature asynchronous. RabbitRPC ensures that remote calls to service actions are invoked asynchronously.

You may also noticed that the action method receive a `CancellationToken` as parameter. The `CancellationToken` is not passed from client to server as a parameter, but rather used to propagate cancellation from client to server. When the `CancellationToken` gets cancelled, the service proxy will broadcast a cancellation notification for the associated request on the management exchange. The sever which is processing the request will cancel the corresponding `CancellationToken` passed down the request processing pipeline, which will cause the request to abort.

## Implementing the Service
The `ChatService` is implemented in `Server` project. The implementation is quite straight forward. We just need to implement the service contract and say hello in `HelloAsync`.
```csharp
class ChatService : IChatService
{
    public Task<string> HelloAsync(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Hello {name}!");
    }
}
```

To get the service up and running, we need a service host.
```csharp
static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddMessagePackSerializationProvider();
            services.AddRabbitMQConnectionProvider("amqp://guest:guest@localhost/");
            services.AddRabbitServiceHost(options =>
            {
                options.AddServicesFromAssembly();
            });
        });

CreateHostBuilder(args).Build().Run();
```
 The above code configures and starts a service host, which will discover and host all service implementations in current assembly.
 
 The service host will be connecting to a RabbitMQ server on `localhost` and will be using MessagePack to serialize request and response messages.

## Calling the Service
Now it's time to call the service. First, we need to configure the client to use the same serialization provider and RabbitMQ instance as the server so that they can communicate:
```csharp
services.AddMessagePackSerializationProvider();
services.AddRabbitMQConnectionProvider("amqp://guest:guest@localhost/");
```
Then we register service contracts defined in `Shared` project for proxy generation:
```csharp
services.AddRabbitServiceClient(typeof(IChatService).Assembly);
```
And that's it, now we can say hello to the server:
```csharp
while (!cancellationToken.IsCancellationRequested)
{
    try
    {
        await Console.Out.WriteAsync("Enter your name to say hello: ");
        var msg = await Console.In.ReadLineAsync();
        var greeting = await _chatService.HelloAsync(msg!, cancellationToken);
        await Console.Out.WriteLineAsync(greeting);
    }
    catch (Exception ex)
    {
        await Console.Error.WriteLineAsync(ex.ToString());
    }
}
```
## Running the Example
To run the example, you'll need a working RabbitMQ server instance. You can install one by using RabbitMQ installer or Docker (recommended).

Now start a server and a client, enter something in the client and see whether the `ChatService` works as expected.

You can also start as many servers and clients you want, to see how the requests are load balanced.