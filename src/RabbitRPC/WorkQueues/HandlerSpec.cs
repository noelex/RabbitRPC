using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.WorkQueues
{
    internal class HandlerSpec
    {
        public HandlerSpec(Type workItemType, Type handlerType, WorkItemHandlerOptions options)
            => (WorkItemType, HandlerType, Options) = (workItemType, handlerType, options);

        public Type WorkItemType { get; }

        public Type HandlerType { get; }

        public WorkItemHandlerOptions Options { get; }
    }
}
