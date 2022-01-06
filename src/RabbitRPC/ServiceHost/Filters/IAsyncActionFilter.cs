using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitRPC.ServiceHost.Filters
{
    public delegate Task<IActionExecutedContext> ActionExecutionDelegate();

    public interface IAsyncActionFilter: IFilterMetadata
    {
        Task OnActionExecutionAsync(IActionExecutingContext context, ActionExecutionDelegate next);
    }
}
