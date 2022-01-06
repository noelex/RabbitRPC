using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost.Filters
{
    public interface IOrderedFilter:IFilterMetadata
    {
        int Order { get; set; }
    }
}
