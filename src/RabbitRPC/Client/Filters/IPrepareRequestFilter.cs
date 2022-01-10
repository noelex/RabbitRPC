using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Client.Filters
{
    public interface IPrepareRequestFilter:IProxyFilterMetadata
    {
        void OnPrepareRequest(IPrepareRequestContext context);
    }
}
