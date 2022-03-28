using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitRPC;
using RabbitRPC.ServiceHost;
using RabbitRPC.ServiceHost.Filters.Internal;
using RabbitRPC.States;
using RabbitRPC.States.FileSystem;
using RabbitRPC.States.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitServiceHostServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitServiceHost(this IServiceCollection services, Action<RabbitServiceHostOptions>? setupAction=null)
        {
            services.TryAddEventBus();

            services.TryAddSingleton<IStateContextFactory, InMemoryStateContextFactory>();

            var opt = new RabbitServiceHostOptions();
            opt.Services = services;
            opt.AddFilter(new DefaultParameterBindingFilter(), int.MinValue);
            opt.AddFilter(new InjectServicePropertiesFilter(), int.MinValue);
          
            setupAction?.Invoke(opt);
            
            services.Configure<RabbitServiceHostOptions>(options =>
            {
                options.Filters = opt.Filters;
                options.ServiceDescriptors = opt.ServiceDescriptors;
            });

            services.AddScoped(sp =>
            {
                var factory = sp.GetRequiredService<IStateContextFactory>();
                return factory.CreateStateContext(sp.GetRequiredService<ICallContextAccessor>().CallContext!.ServiceInstance!.GetType().FullName);
            });

            return services
                .AddSingleton<ICallContextAccessor, CallContextAccessor>()
                .AddHostedService<RabbitServiceHost>();
        }

        public static IServiceCollection AddFileSystemStateContext(this IServiceCollection services, string location)
            => services.AddSingleton<IStateContextFactory>(sp=>new FileSystemStateContextFactory(location, sp.GetRequiredService<IServiceProvider>()));

        public static IServiceCollection AddInMemoryStateContext(this IServiceCollection services)
            => services.AddSingleton<IStateContextFactory, InMemoryStateContextFactory>();
    }
}
