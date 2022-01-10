using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Client.Filters
{
    internal class RequestCompletedContext : RequestContext, IRequestCompletedContext
    {
        public RequestCompletedContext(IRequestContext context)
            :base(context)
        {
        }

        /// <summary>
        ///  Gets or sets an indication that an requst filter short-circuited the request and the request filter pipeline.
        /// </summary>
        public bool Canceled { get; set; }

        public RequestStatus Status { get; set; }

        public bool ExceptionHandled { get; set; }

        public Exception? Exception { get; set; }

        public object? Result { get; set; }
    }
}
