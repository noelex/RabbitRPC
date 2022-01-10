using RabbitRPC.Client;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using RabbitRPC;
using RabbitMQ.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitServiceClientServiceCollectionExtensions
    {
        private static readonly MethodInfo _createProxy =
            typeof(RabbitServiceClientFactory).GetMethods().Where(x => x.Name == "CreateProxy" && x.GetParameters().Length == 2).First();

        public static IServiceCollection AddRabbitServiceClient(this IServiceCollection services, Type serviceInterface, Action<RabbitServiceClientOptionBuilder>? configure=null)
        {
            services.TryAddEventBus();

            if (!serviceInterface.IsInterface)
            {
                throw new ArgumentException($"{serviceInterface.FullName} is not an interface.");
            }

            if (!typeof(IRabbitService).IsAssignableFrom(serviceInterface))
            {
                throw new ArgumentException($"Service interface must implement IRabbitService interface.");
            }

            services.AddSingleton(serviceInterface, sp => _createProxy.MakeGenericMethod(new[] { serviceInterface }).Invoke(null, new object?[] { sp, configure }));
            return services;
        }

        public static IServiceCollection AddRabbitServiceClient<T>(this IServiceCollection services, Action<RabbitServiceClientOptionBuilder>? configure = null) where T : IRabbitService
            => services.AddRabbitServiceClient(typeof(T), configure);

        public static IServiceCollection AddRabbitServiceClient(this IServiceCollection services, Type[] serviceInterfaces, Action<RabbitServiceClientOptionBuilder>? configure = null)
        {
            foreach (var serviceType in serviceInterfaces)
            {
                services.AddRabbitServiceClient(serviceType, configure);
            }
            return services;
        }

        public static IServiceCollection AddRabbitServiceClient(this IServiceCollection services, Assembly serviceAssembly, Action<RabbitServiceClientOptionBuilder>? configure = null)
        {
            var types = serviceAssembly.GetExportedTypes().Where(x => x.IsInterface && typeof(IRabbitService).IsAssignableFrom(x)).ToArray();
            return services.AddRabbitServiceClient(types, configure);
        }

        public static IServiceCollection AddRabbitServiceClient(this IServiceCollection services, Action<RabbitServiceClientOptionBuilder>? configure = null)
        {
            return services.AddRabbitServiceClient(Assembly.GetCallingAssembly(), configure);
        }
    }
}
