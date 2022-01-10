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
        public ResponseReceivedContext(IRequestContext requestContext, IBasicProperties responsePropterties, ReadOnlyMemory<byte> responseData, IResponseMessageBody body)
            : base(requestContext)
        {
            ResponsePropterties = responsePropterties;
            RawData = responseData;
            Body = body;
        }

        public IBasicProperties ResponsePropterties { get; set; }

        public ReadOnlyMemory<byte> RawData { get; set; }

        public IResponseMessageBody Body { get; set; }
    }
}
