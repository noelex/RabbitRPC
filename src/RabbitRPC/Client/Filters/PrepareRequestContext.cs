using RabbitMQ.Client;
using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Client.Filters
{
    class PrepareRequestContext : RequestContext, IPrepareRequestContext
    {
        public PrepareRequestContext(IRequestContext context, IBasicProperties properties, IRequestMessageBody body)
            : base(context)
        {
            RequestProperties = properties;
            RequestBody = body;
        }

        public IBasicProperties RequestProperties { get; }

        public IRequestMessageBody RequestBody { get; }
    }
}
