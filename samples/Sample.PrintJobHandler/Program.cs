// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitRPC.WorkQueues;
using Sample.Shared;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddMessagePackSerializationProvider();
            services.AddRabbitMQConnectionProvider("amqp://guest:guest@localhost/");

            services.AddWorkQueue(x=>x.AddHandler<PrintJob, PrintJobHandler>(options=>
            {
                options.ConcurrencyMode = BatchConcurrencyMode.Isolated;
                options.BatchTimeout = 0;
                options.BatchSize = 32;
            }));

            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace));
        });

CreateHostBuilder(args).Build().Run();

public class PrintJobHandler : IWorkItemHandler<PrintJob>
{
    public async Task ProcessAsync(ReadOnlyMemory<WorkItem<PrintJob>> items, CancellationToken cancellationToken)
    {
        for(var i=0; i<items.Length; i++)
        {
            Console.WriteLine(items.Span[i].Value.Text);
            items.Span[i].IsDone = true;
        }
        await Task.Delay(100, cancellationToken);
    }
}