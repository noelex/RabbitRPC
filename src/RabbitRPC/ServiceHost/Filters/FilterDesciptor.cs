using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost.Filters
{
    public class FilterDescriptor
    {
        public FilterDescriptor( IFilterMetadata filter, int order, int scope)
            => (Filter, Order, Scope) = (filter, order, scope);

        public IFilterMetadata Filter { get; }

        public int Order { get; }

        public int Scope { get; }
    }
}
