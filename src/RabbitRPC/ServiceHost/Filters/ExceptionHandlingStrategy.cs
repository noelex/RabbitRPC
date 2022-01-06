using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost.Filters
{
    public enum ExceptionHandlingStrategy
    {
        /// <summary>
        /// Propagate the exception to calling client. This is the default behaviour.
        /// </summary>
        PropagateToClient,

        /// <summary>
        /// Ignore the exception and return a default value.
        /// </summary>
        ReturnDefault,

        /// <summary>
        /// Re-execute the RPC pipeline on current replica.
        /// </summary>
        ReExecute,

        /// <summary>
        /// Reject the action invocation request so that it can be re-dispatched by the underlying message broker.
        /// </summary>
        RejectRequest
    }
}
