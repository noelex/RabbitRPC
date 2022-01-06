using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost.Filters
{
    public interface IServiceInstantiationFilter: IFilterMetadata
    {
        void OnInitializeServiceInstance(IActionContext context, IRabbitService serviceInstance);
    }
}
