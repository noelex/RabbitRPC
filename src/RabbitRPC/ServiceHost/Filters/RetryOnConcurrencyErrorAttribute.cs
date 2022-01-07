using RabbitRPC.States;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitRPC.ServiceHost.Filters
{
    /// <summary>
    /// Re-execute the action when <see cref="ConcurrencyException"/> is thrown.
    /// </summary>
    /// <remarks>
    /// This filter should only be used when the concurrency token is retrieved inside the action.
    /// If the token is passed to the action via action parameters or read from cache, attempts to retry will never succeed.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RetryOnConcurrencyErrorAttribute : ActionFilterAttribute
    {
        public int MaxRetries { get; set; } = 10;

        public int BackoffTime { get; set; } = 0;

        public override async Task OnActionExecutionAsync(IActionExecutingContext context, ActionExecutionDelegate next)
        {
            var result = await next();

            if (result.Exception is ConcurrencyException && context.CallContext.ExecutionId <= MaxRetries)
            {
                result.UnhandledExceptionHandlingStrategy = ExceptionHandlingStrategy.ReExecute;
                await Task.Delay(BackoffTime, context.CallContext.RequestAborted);
            }
        }
    }
}
