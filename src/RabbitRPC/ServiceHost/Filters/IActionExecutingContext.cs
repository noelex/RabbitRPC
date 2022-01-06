using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost.Filters
{
    /// <summary>
    /// A context for action filters, specifically <see cref="IActionFilter.OnActionExecuting(IActionExecutingContext)"/> and
    /// <see cref="IAsyncActionFilter.OnActionExecutionAsync(IActionExecutingContext, ActionExecutionDelegate)"/> calls.
    /// </summary>
    public interface IActionExecutingContext : IFilterContext
    {
        /// <summary>
        /// Gets the arguments to pass when invoking the action. Keys are parameter names.
        /// </summary>
        IDictionary<string, object?> ActionArguments { get; }

        /// <summary>
        /// Gets the service instance containing the action.
        /// </summary>
        IRabbitService Service { get; }

        /// <summary>
        /// Gets or sets the result of the Action.
        /// Setting <see cref="Result"/> to a non-null value inside an action filter will short-circuit the action and any remaining action filters.
        /// </summary>
        object? Result { get; set; }
    }
}
