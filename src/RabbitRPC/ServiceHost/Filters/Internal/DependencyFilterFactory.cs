using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost.Filters.Internal
{
    internal class DependencyFilterFactory : IFilterFactory
    {
        private readonly Type _filterType;

        public DependencyFilterFactory(Type filterType)
        {
            _filterType = filterType;
        }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return (IFilterMetadata)serviceProvider.GetRequiredService(_filterType);
        }
    }
}
