// See https://aka.ms/new-console-template for more information
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitRPC;
using RabbitRPC.ServiceHost;
using RabbitRPC.ServiceHost.Filters;
using Shared;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddMessagePackSerializationProvider();
            services.AddRabbitMQConnectionProvider("amqp://guest:guest@localhost/");
            services.AddEventBus();

            services.AddHostedService<TimerService>();
        });

CreateHostBuilder(args).Build().Run();

class TimerService : IHostedService
{
    private readonly IRabbitEventBus _eventBus;
    private readonly CancellationTokenSource _cts = new();

    public TimerService(IRabbitEventBus eventBus) => _eventBus = eventBus;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = RunAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        return Task.CompletedTask;
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        var eventId = 0L;
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellationToken);

            _eventBus.Publish(new TimerEvent { Id = eventId++ });
        }
    }
}
