using RabbitMQ.Client;
using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Client.Filters
{
    public interface IResponseReceivedContext : IRequestContext
    {
        IBasicProperties ResponsePropterties { get; }

        ReadOnlyMemory<byte> RawData { get; }

        IResponseMessageBody Body { get; }
    }

    internal class ResponseReceivedContext : RequestContext, IResponseReceivedContext
    {
        public ResponseReceivedContext(IRequestContext requestContext)
            : base(requestContext)
        {
        }

        public IBasicProperties ResponsePropterties { get; set; } = null!;

        public ReadOnlyMemory<byte> RawData { get; set; } = null!;

        public IResponseMessageBody Body { get; set; } = null!;
    }
}
