using RabbitMQ.Client;
using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.Client
{
    internal class RequestContext : IRequestContext
    {
        public RequestContext(MethodInfo targetMethod, object?[] arguments, string requestId, string responseQueueName)
        {
            ActionMethod = targetMethod;
            ServiceInterface = targetMethod.DeclaringType;
            ActionName = targetMethod.GetCustomAttribute<ActionAttribute>()?.Name ?? targetMethod.Name;
            ServiceName = ServiceInterface.GetCustomAttribute<RabbitServiceAttribute>()?.Name ?? targetMethod.DeclaringType.FullName;
            RequestQueueName = "RabbitRPC:" + ServiceName;
            Arguments = arguments;
            RequestId = requestId;
            ResponseQueueName = responseQueueName;
            ReturnType = targetMethod.ReturnType == typeof(Task) ? typeof(void) : targetMethod.ReturnType.GetGenericArguments()[0];
        }

        public RequestContext(IRequestContext requestContext)
        {
            ActionMethod = requestContext.ActionMethod;
            ServiceInterface = requestContext.ServiceInterface;
            ServiceName = requestContext.ServiceName;
            ActionName = requestContext.ActionName;
            Arguments = requestContext.Arguments;
            ResponseQueueName = requestContext.ResponseQueueName;
            RequestId = requestContext.RequestId;
            ReturnType = requestContext.ReturnType;
            RequestQueueName = requestContext.RequestQueueName;
        }

        public MethodInfo ActionMethod { get;  } 

        public Type ServiceInterface { get; } 

        public string ServiceName { get;  } 

        public string ActionName { get; } 

        public object?[] Arguments { get;} 

        public string ResponseQueueName { get;} 

        public string RequestId { get;} 

        public Type ReturnType { get; } 

        public string RequestQueueName { get; }

        public override string ToString()
        {
            return $"{{rid:{RequestId}, target:{ServiceName}.{ActionName}}}";
        }
    }
}
