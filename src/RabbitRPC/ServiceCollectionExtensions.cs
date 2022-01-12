using RabbitMQ.Client;
using RabbitRPC;
using RabbitRPC.WorkQueues;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQConnectionProvider(this IServiceCollection services, string rabbitMQServerUri)
            => services.AddSingleton<IRabbitMQConnectionProvider>(sp => new DefaultRabbitMQConnectionProvider(new ConnectionFactory
            {
                Uri = new Uri(rabbitMQServerUri),
                DispatchConsumersAsync = true
            }));

        public static IServiceCollection AddRabbitMQConnectionProvider(this IServiceCollection services, IConnectionFactory connectionFactory)
            => services.AddSingleton<IRabbitMQConnectionProvider>(sp => new DefaultRabbitMQConnectionProvider(connectionFactory));

        public static IServiceCollection AddRabbitMQConnectionProvider<T>(this IServiceCollection services) where T : class, IRabbitMQConnectionProvider
            => services.AddSingleton<IRabbitMQConnectionProvider, T>();

        public static IServiceCollection AddWorkQueue(this IServiceCollection services, Action<WorkQueueOptionsBuilder>? configure = null)
        {
            services.TryAddEventBus();

            services.AddSingleton<RabbitWorkQueue>();
            services.AddSingleton<IWorkQueue, RabbitWorkQueue>(x => x.GetRequiredService<RabbitWorkQueue>());
            services.AddHostedService(x => x.GetRequiredService<RabbitWorkQueue>());

            var builder = new WorkQueueOptionsBuilder(services);
            configure?.Invoke(builder);

            return services.AddSingleton(builder.Build());
        }

        public static IServiceCollection AddEventBus(this IServiceCollection services) => services.TryAddEventBus();

        internal static bool TryAddSingleton<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : class, TService
            where TService : class
        {
            if (services.Any(x => x.ServiceType == typeof(TService)))
            {
                return false;
            }

            services.AddSingleton<TService, TImplementation>();
            return true;
        }

        internal static bool TryAddSingleton<TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory)
            where TService : class
        {
            if (services.Any(x => x.ServiceType == typeof(TService)))
            {
                return false;
            }

            services.AddSingleton(factory);
            return true;
        }

        internal static bool TryAddSingleton<TService>(this IServiceCollection services)
            where TService : class
        {
            if (services.Any(x => x.ServiceType == typeof(TService)))
            {
                return false;
            }

            services.AddSingleton<TService>();
            return true;
        }

        internal static IServiceCollection TryAddEventBus(this IServiceCollection services)
        {
            if (services.TryAddSingleton<RabbitEventBus>() && services.TryAddSingleton<IRabbitEventBus>(sp => sp.GetRequiredService<RabbitEventBus>()))
            {
                services.AddSingleton<IHostedEventBusFactory, HostedEventBusFactory>();
                services.AddHostedService(x => x.GetRequiredService<RabbitEventBus>());
            }
            return services;
        }
    }
}
