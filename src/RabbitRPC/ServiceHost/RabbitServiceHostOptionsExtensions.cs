using Microsoft.Extensions.DependencyInjection;
using RabbitRPC.ServiceHost.Filters;
using RabbitRPC.ServiceHost.Filters.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RabbitRPC.ServiceHost
{
    public static class RabbitServiceHostOptionsExtensions
    {
        public static RabbitServiceHostOptions AddService<T>(this RabbitServiceHostOptions options) where T : IRabbitService
        {
            options.AddService(typeof(T));

            return options;
        }

        public static RabbitServiceHostOptions AddService(this RabbitServiceHostOptions options, Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

            var serviceInterfaces = serviceType.GetInterfaces()
                .Where(x => x != typeof(IRabbitService) && typeof(IRabbitService).IsAssignableFrom(x))
                .ToArray();

            options.Services.AddScoped(serviceType);

            foreach (var serviceInterface in serviceInterfaces)
            {
                var name = serviceInterface.GetCustomAttribute<RabbitServiceAttribute>()?.Name ?? serviceInterface.FullName;

                if (options.ServiceDescriptors.ContainsKey(name))
                {
                    throw new ArgumentException($"A service with name '{name}' is already registered.", nameof(serviceType));
                }

                options.ServiceDescriptors.Add(name, new ServiceDescriptor(serviceType.GetInterfaceMap(serviceInterface)));
            }


            return options;
        }

        public static RabbitServiceHostOptions AddServices(this RabbitServiceHostOptions options, IEnumerable<Type> serviceTypes)
        {
            foreach (var serviceType in serviceTypes)
            {
                options.AddService(serviceType);
            }

            return options;
        }

        public static RabbitServiceHostOptions AddServicesFromAssembly(this RabbitServiceHostOptions options, Assembly assembly)
        {
            return options.AddServices(assembly.GetTypes().Where(x => typeof(IRabbitService).IsAssignableFrom(x) && !x.IsAbstract));
        }

        public static RabbitServiceHostOptions AddServicesFromAssembly(this RabbitServiceHostOptions options)
        {
            return options.AddServicesFromAssembly(Assembly.GetCallingAssembly());
        }

        public static RabbitServiceHostOptions AddFilter(this RabbitServiceHostOptions options, IFilterMetadata filter, int order=0)
        {
            options.Filters.Add(new FilterDescriptor(filter, (filter is IOrderedFilter of)?of.Order:order, FilterScope.Global));
            return options;
        }

        public static RabbitServiceHostOptions AddFilter(this RabbitServiceHostOptions options, Type filterType, ServiceLifetime serviceLifetime=ServiceLifetime.Scoped, int order = 0)
        {
            if (!typeof(IFilterMetadata).IsAssignableFrom(filterType) || filterType.IsAbstract)
            {
                throw new ArgumentException("Filter type must be a concrete type that implements IFilterMetadata interface.", nameof(filterType));
            }

            options.Services.Add(new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(filterType, filterType, serviceLifetime));
            options.Filters.Add(new FilterDescriptor(new DependencyFilterFactory(filterType), order, FilterScope.Global));
            return options;
        }

        public static RabbitServiceHostOptions AddFilter<T>(this RabbitServiceHostOptions options, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped, int order = 0)
            where T : IFilterMetadata
        {
            return options.AddFilter(typeof(T), serviceLifetime, order);
        }
    }
}
