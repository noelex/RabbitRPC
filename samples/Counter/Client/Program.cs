using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared;
using System.Diagnostics;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddHostedService<ConsoleServiceHost>();

            services.AddMessagePackSerializationProvider();
            services.AddRabbitMQConnectionProvider("amqp://guest:guest@localhost/");
            services.AddRabbitServiceClient(typeof(ICounterService).Assembly);

            services.AddLogging(x => x.SetMinimumLevel(LogLevel.None));
        });

CreateHostBuilder(args).Build().Run();

class ConsoleServiceHost : IHostedService
{
    private readonly ICounterService _counterService;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public ConsoleServiceHost(ICounterService counterService)
    {
        _counterService = counterService;
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        void PrintUsage()
        {
            Console.WriteLine("Command list: ");
            Console.WriteLine("    i: Increment counter by 100.");
            Console.WriteLine("    d: Decrement counter by 100.");
            Console.WriteLine("    v: Print current value of the counter.");
        }

        PrintUsage();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                Console.Write("> ");
                var cmd = await Console.In.ReadLineAsync();
                var sw = new Stopwatch();

                switch (cmd)
                {
                    case "i":
                        sw.Start();
                        await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => _counterService.IncrementAsync(cancellationToken)));
                        sw.Stop();
                        break;
                    case "d":
                        sw.Start();
                        await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => _counterService.DecrementAsync(cancellationToken)));
                        sw.Stop();
                        break;
                    case "v":
                        sw.Start();
                        Console.WriteLine("Current counter value is " + await _counterService.GetCounterAsync(cancellationToken));
                        sw.Stop();
                        break;
                    default:
                        PrintUsage();
                        continue;
                }

                Console.WriteLine($"Request finished in {sw.Elapsed.TotalMilliseconds:F4}ms.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
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


