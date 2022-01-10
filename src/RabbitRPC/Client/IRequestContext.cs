using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RabbitRPC.Client
{
    public interface IRequestContext
    {
        MethodInfo ActionMethod { get; }

        Type ServiceInterface { get; }

        string ServiceName { get; }

        string ActionName { get; }

        object?[] Arguments { get; }

        string ResponseQueueName { get; }

        string RequestId { get; }

        Type ReturnType { get; }

        string RequestQueueName { get; }
    }
}
