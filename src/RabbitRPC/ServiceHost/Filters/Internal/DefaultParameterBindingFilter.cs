using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RabbitRPC.ServiceHost.Filters.Internal
{
    internal class DefaultParameterBindingFilter : IParameterBindingFilter
    {
        public void OnBindParameters(IActionContext context, IDictionary<string, object?> parameters)
        {
            var ps = context.ActionDescriptor.Parameters;

            for (var i = 0; i < context.ActionDescriptor.Parameters.Count; i++)
            {
                
                var pname = ps[i].Name;

                if (i == ps.Count - 1 && ps[i].ParameterType == typeof(CancellationToken))
                {
                    parameters[pname] = context.CallContext.RequestAborted;
                }
                else
                {
                    parameters[pname] = context.CallContext.RequestBody.GetParameter(i, pname, ps[i].ParameterType);
                }
            }
        }
    }
}
