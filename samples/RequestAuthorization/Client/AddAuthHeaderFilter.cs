using RabbitRPC;
using RabbitRPC.Client.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    internal class AddAuthHeaderFilter : IPrepareRequestFilter
    {
        public string? Name { get; set; }

        public void OnPrepareRequest(IPrepareRequestContext context)
        {
            context.RequestProperties.SetHeader("X-Auth-UserId", Name!);
        }
    }
}
