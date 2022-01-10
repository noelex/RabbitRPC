// See https://aka.ms/new-console-template for more information
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitRPC;
using RabbitRPC.ServiceHost;
using RabbitRPC.ServiceHost.Filters;
using RabbitRPC.WorkQueues;
using Shared;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddMessagePackSerializationProvider();
            services.AddRabbitMQConnectionProvider("amqp://guest:guest@localhost/");
            services.AddWorkQueue(options => options.AddHandler<PrinterJob, PrinterJobHandler>(opt =>
            {
                opt.DegreeOfParallelism = 1;
                opt.BatchTimeout = 1000;
                opt.BatchSize = 10;
            }));

            services.AddLogging(x => x.SetMinimumLevel(LogLevel.Trace));
        });

CreateHostBuilder(args).Build().Run();

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
