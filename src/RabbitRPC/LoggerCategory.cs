using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC
{
    internal static class LoggerCategory
    {
        public const string Hosting = "RabbitRPC.ServiceHost";
        public const string WorkQueue = "RabbitRPC.WorkQueue";
        public const string WorkItemHandler = "RabbitRPC.WorkQueue.{0}";
    }
}
