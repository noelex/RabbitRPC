using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RabbitRPC.Client.Filters.Internal
{
    internal class DefaultPrepareRequestFilter : IPrepareRequestFilter
    {
        public void OnPrepareRequest(IPrepareRequestContext context)
        {
            // Prepare header
            context.RequestProperties.CorrelationId = context.RequestId;
            context.RequestProperties.ReplyTo = context.ResponseQueueName;
            context.RequestProperties.Type = context.ActionName;

            // Prepare request body
            var parameters = context.ActionMethod.GetParameters(); 
            for (var i = 0; i <= parameters.Length; i++)
            {
                if (parameters.Length - 1 == i && context.Arguments[i] is CancellationToken)
                {
                    break;
                }
                context.RequestBody.SetParameter(i, parameters[i].Name, context.Arguments[i]);
            }
        }
    }
}
