using Microsoft.Extensions.DependencyInjection;
using RabbitRPC.ServiceHost;
using RabbitRPC.ServiceHost.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost
{
    public class RabbitServiceHostOptions
    {
        internal IServiceCollection Services { get; set; } = null!;

        public int DispatchConcurrency { get; set; } = 16;

        public IDictionary<string, ServiceDescriptor> ServiceDescriptors { get; set; } = new Dictionary<string, ServiceDescriptor>();

        public IList<FilterDescriptor> Filters { get; set; } = new List<FilterDescriptor>();
    }
}
