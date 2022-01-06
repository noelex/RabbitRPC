using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost.Filters
{
    /// <summary>
    /// A context for action filters, specifically <see cref="IActionFilter.OnActionExecuted(IActionExecutedContext)"/> calls.
    /// </summary>
    public interface IActionExecutedContext : IFilterContext
    {
        /// <summary>
        ///  Gets or sets an indication that an action filter short-circuited the action and the action filter pipeline.
        /// </summary>
        bool Canceled { get; set; }

        /// <summary>
        /// Gets or sets an indication that the <see cref="Exception"/> has been handled.
        /// </summary>
        bool ExceptionHandled { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Exception"/> caught while executing the action or action filters, if any.
        /// </summary>
        Exception? Exception { get; set; }

        /// <summary>
        /// Gets or sets the strategy to be used when an exception is not handled by any filter and user code.
        /// </summary>
        ExceptionHandlingStrategy UnhandledExceptionHandlingStrategy { get; set; }

        /// <summary>
        /// Gets or sets the result of the action.
        /// </summary>
        object? Result { get; set; }

        /// <summary>
        /// Gets the service instance containing the action.
        /// </summary>
        IRabbitService Service { get; }
    }
}
