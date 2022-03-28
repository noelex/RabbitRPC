using RabbitMQ.Client;
using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RabbitRPC.ServiceHost
{
    public interface ICallContext
    {
        string ServiceName { get; }

        string ActionName { get; }

        string HostId { get; }

        string RequestId { get; }

        int ExecutionId { get; }

        IBasicProperties RequestProperties { get; }

        IBasicProperties ResponseProperties { get; }

        IRequestMessageBody RequestBody { get; }

        IResponseMessageBody ResponseBody { get; }

        CancellationToken RequestAborted { get; }

        IServiceProvider RequestServices { get; }

        IDictionary<string, object?> Items { get; }

        object? ServiceInstance { get; }
    }
}
