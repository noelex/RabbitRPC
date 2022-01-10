using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Client.Filters.Internal
{
    internal class FactoryMethodFilterFactory : IProxyFilterFactory
    {
        private readonly Func<IProxyFilterMetadata> _factory;

        public FactoryMethodFilterFactory(Func<IProxyFilterMetadata> factory)
        {
            _factory = factory;
        }

        public IProxyFilterMetadata CreateInstance()
        {
            return _factory();
        }
    }
}
