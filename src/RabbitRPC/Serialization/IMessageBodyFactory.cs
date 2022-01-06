using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Serialization
{
    public interface IMessageBodyFactory
    {
        IRequestMessageBody CreateRequest(string interfaceName, string methodName, int numberOfParameters, object? wrappedRequestObject);

        IResponseMessageBody CreateResponse(string interfaceName, string methodName);
    }
}
