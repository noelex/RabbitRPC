using Microsoft.Extensions.DependencyInjection;
using RabbitRPC.States;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost.Filters.Internal
{
    internal class InjectServicePropertiesFilter : IServiceInitializationFilter
    {
        public void OnInitializeServiceInstance(IActionContext context, IRabbitService serviceInstance)
        {
            if(serviceInstance is RabbitService rs)
            {
                rs.StateContext = context.CallContext.RequestServices.GetRequiredService<IStateContext>();
                rs.EventBus = context.CallContext.RequestServices.GetRequiredService<IRabbitEventBus>();
                rs.CallContext = context.CallContext;
            }
        }
    }
}
