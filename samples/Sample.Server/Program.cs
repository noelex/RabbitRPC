// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitRPC.ServiceHost;
using RabbitRPC.ServiceHost.Filters;
using RabbitRPC.WorkQueues;
using ServiceLib;
using ServiceLib.WorkItems;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddMessagePackSerializationProvider();
            services.AddRabbitMQConnectionProvider("amqp://guest:guest@localhost/");
            services.AddSqliteStateContext("Data Source=states.db");
            services.AddRabbitServiceHost(options =>
            {
                options.AddServicesFromAssembly();
            });
            services.AddWorkQueue();

            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Information));
        });

CreateHostBuilder(args).Build().Run();

class TestService : RabbitService, ITestService
{
    private readonly IWorkQueue _workQueue;

    public TestService(IWorkQueue workQueue)
    {
        _workQueue = workQueue;
    }

    public async Task<long> GetCounterAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        var v = await StateContext.GetAsync<long>("longValue", cancellationToken);

        return v.Value;
    }
    public Task<string> HelloAsync(string name, CancellationToken cancellationToken)
    {
        return Task.FromResult("Hello " + name);
    }

    public Task TestExceptionAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task TestTimeoutAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(60000, cancellationToken);
    }

    public Task<long> AddAsync(int a, long b, CancellationToken cancellationToken)
    {
        return Task.FromResult(a + b);
    }

    public async Task<TestType> GetEnumAsync(CancellationToken cancellationToken = default)
    {
        var v = await StateContext.GetAsync<TestType>("enumState", cancellationToken);
        return v.Value;
    }

    public async Task SetEnumAsync(TestType e, CancellationToken cancellationToken = default)
    {
        await StateContext.PutAsync("enumState", e);
    }

    [RetryOnConcurrencyError]
    public async Task<long> IncrementAsync(int delay, CancellationToken cancellationToken = default)
    {
        await Task.Delay(delay, cancellationToken);
        var v = await StateContext.GetAsync<long>("longValue", cancellationToken);
        var newV = v.Value + 1;
        await StateContext.PutAsync("longValue", newV, v.Version, cancellationToken);

        _workQueue.Enqueue(new PrintJob($"Current counter is {v.Value} (v={v.Version})."));

        return newV;
    }

    [RetryOnConcurrencyError]
    public async Task<long> DecrementAsync(int delay, CancellationToken cancellationToken = default)
    {
        await Task.Delay(delay, cancellationToken);
        var v = await StateContext.GetAsync<long>("longValue", cancellationToken);
        var newV = v.Value - 1;
        await StateContext.PutAsync("longValue", newV, v.Version, cancellationToken);

        _workQueue.Enqueue(new PrintJob($"Current counter is {v.Value} (v={v.Version})."));

        return newV;
    }
}