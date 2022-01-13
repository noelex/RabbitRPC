# BatchPrinter
This is a minimal example demostrating how to use RabbitRPC to perform remote procedure call.

## Defining Work Items
Similar to events, work items in work queues are also POCO objects. We define a `PrinterJob` type to convey the text to print, in `Shared` project.
```csharp
public class PrinterJob
{
    public string? Text {get; set;}
}
```

## Processing Work Items
To handle a specific type of work item, the handler must implement `IWorkItemHandler<T>` interface, where `T` is the type of the work item. In the `BatchPrinter` example, `T` is `PrinterJob` defined in the above section.

Here we implement a `PrinterJob` handler to texts in `PrinterJob` objects:
```csharp
class PrinterJobHandler : IWorkItemHandler<PrinterJob>
{
    public Task ProcessAsync(ReadOnlyMemory<WorkItem<PrinterJob>> items, CancellationToken cancellationToken)
    {
        foreach(var item in items.Span)
        {
            Console.WriteLine(item.Value.Text);
            item.IsDone = true;
        }

        return Task.CompletedTask;
    }
}
```
You may have noticed that we're setting `item.IsDone` to `true` manually in the above code. In RabbitRPC work queues, work items handled by work item handlers must be mark as done explicitly. In case `IsDone` is left as-is (which is `false` by default), RabbitRPC will reject the work item and place it back in to the work queue so that it can be picked up and retried later.

To start receiving work items, we also need to register the handler and configure how are work items dispatch to the handler:
```csharp
services.AddMessagePackSerializationProvider();
services.AddRabbitMQConnectionProvider("amqp://guest:guest@localhost/");
services.AddWorkQueue(options => options.AddHandler<PrinterJob, PrinterJobHandler>(opt =>
{
    opt.DegreeOfParallelism = 1;
    opt.BatchTimeout = 1000;
    opt.BatchSize = 10;
}));
```
There're some options you can tune when registering a handler:
- `DegreeOfParallelism`: The maximum number of batches can be concurrently processed by the handler (either with a single instance or multiple instances).
- `ConcurrencyMode`: When work items are received from the queue, should the dispatcher use a shared `IServiceScope` for all batches or separate `IServiceScope`s for each batch, to instantiate the work item handler.
- `BatchSize`: The maximum number of work items in a single batch. The maximum number of work items can be received once is thus `DegreeOfParallelism * BatchSize`.
- `BatchTimeout`: The maximum amount of time (in milliseconds) to wait until the dispatcher receives maximum number of work items allowed to receive once (which is `DegreeOfParallelism * BatchSize`).
- `BatchBufferSize`: The maximum number of work items allowed to be buffered in memory before they are dispatched to the handler. When the buffer becomes full, disptacher will stop accepting newer work items from the queue until the buffer become available again.

Simply put, the disptacher receives at most `DegreeOfParallelism * BatchSize` work items from the queue, until there's no more items to consume or `BatchTimeout` is reached. These work items are then splitted into batches with maximum `BatchSize` items. Then the dispatcher create one or more `IServiceScope`s corresponding `ConcurrencyMode`, and passes work items in batch by call `ProcessAsync` method on the handler.

After the handler finished processing work items, the disptacher sends ACK or NACK to the work queue according to the value of `IsDone` property of the work item.

## Posting Work Items
To post work items to the work queue, the client also need to register the work queue service:
```csharp
services.AddMessagePackSerializationProvider();
services.AddRabbitMQConnectionProvider("amqp://guest:guest@localhost/");
services.AddWorkQueue();
```
In the `BatchPrinter` example, we'll generate some `PrinterJob`s and post them to the work queue with random delay:
```csharp
Console.Write("Please enter number of jobs to generate (default: 1000): ");
var input = await Console.In.ReadLineAsync();

if(input is null)
{
    continue;
}

if (!int.TryParse(input, out var count))
{
    count = 1000;
}

Console.WriteLine($"Generate and posting {count} jobs...");

for (var i = 0; i < count; i++)
{
    await Task.Delay(RandomNumberGenerator.GetInt32(0,200), cancellationToken);
    _workQueue.Post(new PrinterJob { Text = $"This is printer job #{i}" });
}

Console.WriteLine("Done.");
```
Since the work queue is durable, work items in the queue will stay until they are processed by a handler and marked as done.
## Running the Example
Now to test the exmaple, you can start multiple `PrinterJobGenerator`s and `PrinterJobHandler`s and type enter in the generators to start posting work items. Handlers will then pick up and process work items from the queue in batch.