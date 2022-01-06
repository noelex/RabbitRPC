using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Serialization.MessagePack
{
    internal class MessagePackMessageFactory : IMessageBodyFactory
    {
        public IRequestMessageBody CreateRequest(string interfaceName, string methodName, int numberOfParameters, object? wrappedRequestObject)
        {
            return new MessagePackRequestBody();
        }

        public IResponseMessageBody CreateResponse(string interfaceName, string methodName)
        {
            return new MessagePackResponseBody();
        }
    }
}
