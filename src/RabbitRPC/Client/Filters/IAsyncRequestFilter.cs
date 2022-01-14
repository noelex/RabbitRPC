using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitRPC.Client.Filters
{
    public delegate Task<IRequestCompletedContext> RequestExecutionDelegate();

    public interface IAsyncRequestFilter : IProxyFilterMetadata
    {
        Task OnRequestInvocationAsync(IRequestStartingContext context, RequestExecutionDelegate next);
    }
}
