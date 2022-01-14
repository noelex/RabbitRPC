using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost.Filters
{
    public interface IServiceInitializationFilter: IFilterMetadata
    {
        void OnInitializeServiceInstance(IActionContext context, IRabbitService serviceInstance);
    }
}
