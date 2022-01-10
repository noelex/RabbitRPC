using RabbitMQ.Client;
using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Client.Filters
{
    public interface IPrepareRequestContext:IRequestContext
    {
        IBasicProperties RequestProperties { get; }

        IRequestMessageBody RequestBody { get; }
    }
}
