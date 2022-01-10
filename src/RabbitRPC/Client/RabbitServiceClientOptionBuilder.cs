using Microsoft.Extensions.DependencyInjection;
using RabbitRPC.Client.Filters;
using RabbitRPC.Client.Filters.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitRPC.Client
{
    public class RabbitServiceClientOptionBuilder
    {
        public RabbitServiceClientOptionBuilder(Type serviceInterface)
        {
            ServiceInterface = serviceInterface;
        }

        public Type ServiceInterface { get; }

        public TimeSpan RequestTimeout { get; set; }

        public IList<ProxyFilterDescriptor> Filters { get; } = new List<ProxyFilterDescriptor>();

        public RabbitServiceClientOptionBuilder AddFilter(IProxyFilterMetadata filter, int order = 0)
        {
            Filters.Add(new ProxyFilterDescriptor(filter, filter is IOrderedFilter o ? o.Order : order));
            return this;
        }

        public RabbitServiceClientOptionBuilder AddFilter<T>(Func<T> factory, int order = 0) where T : class, IProxyFilterMetadata
        {
            Filters.Add(new ProxyFilterDescriptor(new FactoryMethodFilterFactory(factory), order));
            return this;
        }

        public RabbitServiceClientOptionBuilder WithRequestTimeout(TimeSpan timeout)
        {
            RequestTimeout= timeout;
            return this;
        }

        public RabbitServiceClientOptions Build()
        {
            var result = new RabbitServiceClientOptions();
            result.RequestTimeout = RequestTimeout;
            foreach (var filter in Filters)
            {
                result.Filters.Add(filter);
            }

            return result;
        }
    }
}
