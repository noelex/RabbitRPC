using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Serialization.NewtonsoftJson
{
    internal class JsonMessageFactory : IMessageBodyFactory
    {
        public IRequestMessageBody CreateRequest(string interfaceName, string methodName, int numberOfParameters, object? wrappedRequestObject)
        {
            return new JsonRequestBody();
        }

        public IResponseMessageBody CreateResponse(string interfaceName, string methodName)
        {
            return new JsonResponseBody();
        }
    }
}
