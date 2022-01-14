using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Client.Filters
{
    internal class RequestStartingContext : RequestContext, IRequestStartingContext
    {
        public RequestStartingContext(IRequestContext context)
            : base(context)
        {
        }

        public Exception? Exception { get; set; }

        public object? Result { get; set; }
    }
}
