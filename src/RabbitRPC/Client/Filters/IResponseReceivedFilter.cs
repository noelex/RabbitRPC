using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Client.Filters
{
    public interface IResponseReceivedFilter : IProxyFilterMetadata
    {
        void OnResponseReceived(IResponseReceivedContext context);
    }
}
