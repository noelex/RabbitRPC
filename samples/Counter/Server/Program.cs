// See https://aka.ms/new-console-template for more information
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitRPC.ServiceHost;
using RabbitRPC.ServiceHost.Filters;
using Shared;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddMessagePackSerializationProvider();
            services.AddRabbitMQConnectionProvider("amqp://guest:guest@localhost/");
            services.AddEntityFrameworkCoreStateContext(options => options.UseSqlite("Data Source=states.db"));
            //services.AddEntityFrameworkCoreStateContext(options => options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Database=RabbitRPCStates"));
            services.AddRabbitServiceHost(options =>
            {
                options.AddServicesFromAssembly();
            });
        });

CreateHostBuilder(args).Build().Run();

class CounterService : RabbitService, ICounterService
{
    public override void OnActionExecuted(IActionExecutedContext context)
    {
        if (context.Exception is SqlException sqle)
        {
            Console.WriteLine($"Error {sqle.ErrorCode}: {sqle.Message}");
        }
    }

    [RetryOnConcurrencyError]
    public async Task<long> IncrementAsync(CancellationToken cancellationToken = default)
    {
        var val = await StateContext.GetAsync<long>("counter", cancellationToken);
        var newValue = val.Value + 1;
        await StateContext.PutAsync("counter", newValue, val.Version, cancellationToken);

        return newValue;
    }

    [RetryOnConcurrencyError]
    public async Task<long> DecrementAsync(CancellationToken cancellationToken = default)
    {
        var val = await StateContext.GetAsync<long>("counter", cancellationToken);
        var newValue = val.Value - 1;
        await StateContext.PutAsync("counter", newValue, val.Version, cancellationToken);

        return newValue;
    }


    public async Task<long> GetCounterAsync(CancellationToken cancellationToken = default)
    {
        var val = await StateContext.GetAsync<long>("counter", cancellationToken);
        return val.Value;
    }
}
