// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitRPC.ServiceHost;
using Shared;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddMessagePackSerializationProvider();
            services.AddRabbitMQConnectionProvider("amqp://guest:guest@localhost/");
            services.AddRabbitServiceHost(options =>
            {
                options.AddServicesFromAssembly();
            });
        });

CreateHostBuilder(args).Build().Run();

class ChatService : IChatService
{
    public Task<string> HelloAsync(string name, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Hello {name}!");
    }
}
