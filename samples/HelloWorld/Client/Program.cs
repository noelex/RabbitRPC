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
            services.AddRabbitServiceClient(typeof(IChatService).Assembly);

            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
        });

CreateHostBuilder(args).Build().Run();

class ConsoleServiceHost : IHostedService
{
    private readonly IChatService _chatService;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public ConsoleServiceHost(IChatService chatService)
    {
        _chatService = chatService;
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
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
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = RunAsync(_cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }
}


