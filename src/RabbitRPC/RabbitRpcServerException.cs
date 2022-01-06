using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC
{
    [Serializable]
    public class RabbitRpcServerException:Exception
    {
        public RabbitRpcServerException(string message) : base(message) { }
    }
}
