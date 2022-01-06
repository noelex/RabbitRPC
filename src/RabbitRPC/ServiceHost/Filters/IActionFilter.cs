using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost.Filters
{
    public interface IActionFilter : IFilterMetadata
    {
        void OnActionExecuted(IActionExecutedContext context);

        void OnActionExecuting(IActionExecutingContext context);
    }
}
