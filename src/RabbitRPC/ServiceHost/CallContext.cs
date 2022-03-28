using RabbitMQ.Client;
using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RabbitRPC.ServiceHost
{
    internal class CallContext : ICallContext
    {
        public virtual string ServiceName { get; set; } = null!;

        public virtual string ActionName { get; set; } = null!;

        public virtual IBasicProperties RequestProperties { get; set; } = null!;

        public virtual IBasicProperties ResponseProperties { get; set; } = null!;

        public virtual IRequestMessageBody RequestBody { get; set; } = null!;

        public virtual IResponseMessageBody ResponseBody { get; set; } = null!;

        public virtual CancellationToken RequestAborted { get; set; }

        public virtual IServiceProvider RequestServices { get; set; } = null!;

        public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();

        public string HostId { get; set; }= null!;

        public string RequestId { get; set; } = null!;

        public int ExecutionId { get; set; }

        public object? ServiceInstance { get; set; }
    }
}
