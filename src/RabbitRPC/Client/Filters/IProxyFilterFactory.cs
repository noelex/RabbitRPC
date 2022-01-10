using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Client.Filters
{
    public interface IProxyFilterFactory:IProxyFilterMetadata
    {
        IProxyFilterMetadata CreateInstance();
    }
}
