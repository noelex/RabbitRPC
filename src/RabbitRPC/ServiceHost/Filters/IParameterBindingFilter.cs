using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RabbitRPC.ServiceHost.Filters
{
    public interface IParameterBindingFilter : IFilterMetadata
    {
        void OnBindParameters(IActionContext context, IDictionary<string, object?> parameters);
    }
}
