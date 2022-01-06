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
    public static class RabbitServiceProxyServiceCollectionExtensions
    {
        private static readonly MethodInfo _createProxy =
            typeof(RpcServiceProxyFactory).GetMethods().Where(x => x.Name == "CreateProxy" && x.GetParameters().Length == 1).First();

        public static IServiceCollection AddRabbitServiceProxy(this IServiceCollection services, Type serviceInterface)
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

            typeof(RpcServiceProxyFactory).GetMethods().Where(x => x.Name == "CreateProxy" && x.GetParameters().Length == 1).First();
            services.AddSingleton(serviceInterface, sp => _createProxy.MakeGenericMethod(new[] { serviceInterface }).Invoke(null, new object[] { sp }));
            return services;
        }

        public static IServiceCollection AddRabbitServiceProxy<T>(this IServiceCollection services) where T : IRabbitService
            => services.AddRabbitServiceProxy(typeof(T));

        public static IServiceCollection AddRabbitServiceProxy(this IServiceCollection services, Type[] serviceInterfaces)
        {
            foreach (var serviceType in serviceInterfaces)
            {
                services.AddRabbitServiceProxy(serviceType);
            }
            return services;
        }

        public static IServiceCollection AddRabbitServiceProxy(this IServiceCollection services, Assembly serviceAssembly)
        {
            var types = serviceAssembly.GetExportedTypes().Where(x => x.IsInterface && typeof(IRabbitService).IsAssignableFrom(x)).ToArray();
            return services.AddRabbitServiceProxy(types);
        }

        public static IServiceCollection AddRabbitServiceProxy(this IServiceCollection services)
        {
            return services.AddRabbitServiceProxy(Assembly.GetCallingAssembly());
        }
    }
}
