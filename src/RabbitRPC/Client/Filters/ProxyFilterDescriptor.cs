using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Client.Filters
{
    public class ProxyFilterDescriptor
    {
        public ProxyFilterDescriptor(IProxyFilterMetadata filter, int order)
            => (Filter, Order) = (filter, order);

        public IProxyFilterMetadata Filter { get; }

        public int Order { get; }
    }
}
