using RabbitRPC.States;
using System;
using System.Collections.Generic;
using System.Text;

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

        public override void OnActionExecuted(IActionExecutedContext context)
        {
            if (context.Exception is ConcurrencyException && context.CallContext.ExecutionId <= MaxRetries)
            {
                context.UnhandledExceptionHandlingStrategy = ExceptionHandlingStrategy.ReExecute;
            }
        }

        public override void OnActionExecuting(IActionExecutingContext context)
        {
            
        }
    }
}
