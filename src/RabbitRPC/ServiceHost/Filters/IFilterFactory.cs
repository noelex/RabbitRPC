using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost.Filters
{
    public interface IFilterFactory : IFilterMetadata
    {
        IFilterMetadata CreateInstance(IServiceProvider serviceProvider);
    }
}
