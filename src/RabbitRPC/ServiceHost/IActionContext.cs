using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost
{
    public interface IActionContext
    {
        ActionDescriptor ActionDescriptor { get; }

        ServiceDescriptor ServiceDescriptor { get; }

        ICallContext CallContext { get; }
    }
}
