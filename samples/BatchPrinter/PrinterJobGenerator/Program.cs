using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitRPC;
using RabbitRPC.WorkQueues;
using Shared;
using System.Diagnostics;
using System.Security.Cryptography;

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
    private readonly IWorkQueue _workQueue;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public ConsoleServiceHost(IWorkQueue workQueue) => _workQueue = workQueue;

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

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
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
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}


