using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitRPC;
using Shared;
using System.Diagnostics;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddHostedService<ConsoleServiceHost>();

            services.AddMessagePackSerializationProvider();
            services.AddRabbitMQConnectionProvider("amqp://guest:guest@localhost/");
            services.AddWorkQueue();

            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
        });

CreateHostBuilder(args).Build().Run();

class ConsoleServiceHost : IHostedService
{
    private readonly IRabbitEventBus _eventBus;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public ConsoleServiceHost(IRabbitEventBus eventBus) => _eventBus = eventBus;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _eventBus.Observe<TimerEvent>().Subscribe(e => Console.WriteLine($"Received TimerEvent {{Id={e.Id}}}"), _cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }
}


