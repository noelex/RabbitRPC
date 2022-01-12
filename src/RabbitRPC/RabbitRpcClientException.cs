using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RabbitRPC
{
    [Serializable]
    public class RabbitRpcClientException : Exception
    {
        public RabbitRpcClientException(string message) : base(message) { }

        public RabbitRpcClientException(SerializationInfo si, StreamingContext context) : base(si, context) { }
    }
}
