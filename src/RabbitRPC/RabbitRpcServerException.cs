using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RabbitRPC
{
    [Serializable]
    public class RabbitRpcServerException : Exception
    {
        public RabbitRpcServerException(string message) : base(message) { }

        public RabbitRpcServerException(SerializationInfo si, StreamingContext context) : base(si, context) { }
    }
}
