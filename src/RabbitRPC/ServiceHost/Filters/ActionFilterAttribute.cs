using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitRPC.ServiceHost.Filters
{
    /// <summary>
    /// An abstract filter that asynchronously surrounds execution of the action and the action result.
    /// Subclasses should override <see cref="OnActionExecuting(IActionExecutingContext)"/>, <see cref="OnActionExecuted(IActionExecutedContext)"/> or
    /// <see cref="OnActionExecutionAsync(IActionExecutingContext, ActionExecutionDelegate)"/> but not
    /// <see cref="OnActionExecutionAsync(IActionExecutingContext, ActionExecutionDelegate)"/> and either of the other two.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public abstract class ActionFilterAttribute : Attribute, IActionFilter, IAsyncActionFilter, IOrderedFilter
    {
        public int Order { get; set; }

        public virtual void OnActionExecuted(IActionExecutedContext context)
        {
        }

        public virtual void OnActionExecuting(IActionExecutingContext context)
        {
        }

        public virtual async Task OnActionExecutionAsync(IActionExecutingContext context, ActionExecutionDelegate next)
        {
            OnActionExecuting(context);
            var result=await next();
            OnActionExecuted(result);
        }
    }
}
