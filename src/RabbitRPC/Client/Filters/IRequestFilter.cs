using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Client.Filters
{
    public interface IRequestFilter:IProxyFilterMetadata
    {
        void OnRequestStarting(IRequestContext context);

        void OnRequestCompleted(IRequestCompletedContext context);
    }
}
