using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Client.Filters
{
    public interface IRequestStartingContext
    {
        /// <summary>
        /// Gets or sets the result of the Action.
        /// Setting <see cref="Result"/> to a non-null value inside an action filter will short-circuit the action and any remaining action filters.
        /// </summary>
        object? Result { get; set; }

        /// <summary>
        /// Gets or sets the exception of the Action.
        /// Setting <see cref="Exception"/> to a non-null value inside an action filter will short-circuit the action and any remaining action filters.
        /// </summary>
        Exception? Exception { get; set; }
    }
}
