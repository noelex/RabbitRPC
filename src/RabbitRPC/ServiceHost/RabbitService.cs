using RabbitRPC.ServiceHost.Filters;
using RabbitRPC.States;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitRPC.ServiceHost
{
    /// <summary>
    /// Base class for RabbitRPC services.
    /// </summary>
    public abstract class RabbitService: IFilterMetadata, IAsyncActionFilter, IActionFilter, IServiceInitializationFilter, IParameterBindingFilter
    {
        public IStateContext StateContext { get; set; } = null!;

        public IRabbitEventBus EventBus { get; set; } = null!;

        public ICallContext CallContext { get; set; } = null!;

        public virtual void OnActionExecuted(IActionExecutedContext context)
        {

        }

        public virtual void OnActionExecuting(IActionExecutingContext context)
        {
            
        }

        public virtual async Task OnActionExecutionAsync(IActionExecutingContext context, ActionExecutionDelegate next)
        {
            OnActionExecuting(context);
            var result = await next();
            OnActionExecuted(result);
        }

        public virtual void OnBindParameters(IActionContext context, IDictionary<string, object?> parameters)
        {
            
        }

        public virtual void OnInitializeServiceInstance(IActionContext context, IRabbitService serviceInstance)
        {
            
        }
    }
}
