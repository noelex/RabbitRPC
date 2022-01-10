using RabbitRPC.Client.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Client
{
    public class RabbitServiceClientOptions
    {
        public IList<ProxyFilterDescriptor> Filters { get; set; } = new List<ProxyFilterDescriptor>();

        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(1);
    }
}
