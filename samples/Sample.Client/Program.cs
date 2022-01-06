using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitRPC;
using ServiceLib;
using System.Diagnostics;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddHostedService<ConsoleServiceHost>();

            services.AddMessagePackSerializationProvider();
            services.AddRabbitMQConnectionProvider("amqp://guest:guest@localhost/");
            services.AddRabbitServiceProxy(typeof(ITestService).Assembly);
        });

CreateHostBuilder(args).Build().Run();

class ConsoleServiceHost : IHostedService, IObserver<ServerEvent>
{
    private readonly ITestService _testService;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly IRabbitEventBus _eventBus;
    private readonly ILogger _logger;

    public ConsoleServiceHost(ITestService testService, IRabbitEventBus eventBus, ILogger<ConsoleServiceHost> logger)
    {
        _eventBus = eventBus;
        _testService = testService;
        _logger = logger;

        eventBus.Observe<ServerEvent>().Subscribe(this);
    }

    public void OnCompleted()
    {
        
    }

    public void OnError(Exception error)
    {
        
    }

    public void OnNext(ServerEvent value)
    {
        _logger.LogInformation("Received ServerEvent.");
    }

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
        await Task.Yield();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var msg = await Console.In.ReadLineAsync();
                if(msg == "ex")
                {
                    await _testService.TestExceptionAsync(cancellationToken);
                }
                 else if(msg == "ev")
                {
                    _eventBus.Publish(new ClientEvent());
                }
                else if (msg == "to")
                {
                    var tok=CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    tok.CancelAfter(2000);
                    await _testService.TestTimeoutAsync(tok.Token);
                }
                else if (msg == "add")
                {
                    _logger.LogInformation("ADD: "+await _testService.AddAsync(10, 1000L, CancellationToken.None));
                }
                else if (msg == "ge")
                {
                    _logger.LogInformation("GetEnum: " + await _testService.GetEnumAsync());
                }
                else if (msg == "se")
                {
                     await _testService.SetEnumAsync(TestType.Large);
                }
                else if (msg == "incr")
                {
                    _logger.LogInformation("INCR: " + await _testService.IncrementAsync(1000));
                }
                else if (msg == "incr_many")
                {
                    var sw = Stopwatch.StartNew();
                    await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => _testService.IncrementAsync(0)));
                    sw.Stop();
                    _logger.LogInformation($"OK. ({sw.Elapsed.TotalMilliseconds:F2} ms)");
                }
                else if (msg == "decr_many")
                {
                    var sw = Stopwatch.StartNew();
                    await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => _testService.DecrementAsync(0)));
                    sw.Stop();
                    _logger.LogInformation($"OK. ({sw.Elapsed.TotalMilliseconds:F2} ms)" );
                }
                else if (msg == "incr_nowait")
                {
                    _logger.LogInformation("INCR: " + await _testService.IncrementAsync(0));
                }
                else if (msg == "decr")
                {
                    _logger.LogInformation("DECR: " + await _testService.DecrementAsync(1000));
                }
                else if (msg == "decr_nowait")
                {
                    _logger.LogInformation("DECR: " + await _testService.DecrementAsync(0));
                }
                else if (msg == "counter")
                {
                    _logger.LogInformation("COUNTER: " + await _testService.GetCounterAsync());
                }
                else if (msg == "counter_many")
                {
                    var sw = Stopwatch.StartNew();
                    await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => _testService.GetCounterAsync()));
                    sw.Stop();
                    _logger.LogInformation($"OK. ({sw.Elapsed.TotalMilliseconds:F2} ms)");
                }
                else
                {
                    _logger.LogInformation(await _testService.HelloAsync(msg!, cancellationToken));
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
    }
}
