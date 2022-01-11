// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitRPC.ServiceHost;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
            services.AddMessagePackSerializationProvider();
            services.AddRabbitMQConnectionProvider(hostContext.Configuration.GetConnectionString("rabbitmq"));
            services.AddRabbitServiceHost(options =>
            {
                options.AddServicesFromAssembly();
            });
        });

CreateHostBuilder(args).Build().Run();
