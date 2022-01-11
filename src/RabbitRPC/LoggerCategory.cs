using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC
{
    internal static class LoggerCategory
    {
        public const string Hosting = "RabbitRPC.ServiceHost";
        public const string EventBus = "RabbitRPC.EventBus";
        public const string WorkQueue = "RabbitRPC.WorkQueue";
        public const string Proxy = "RabbitRPC.Proxy";
        public const string WorkItemHandler = "RabbitRPC.WorkQueue.{0}";
    }
}
