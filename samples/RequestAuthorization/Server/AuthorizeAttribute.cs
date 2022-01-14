using Microsoft.Extensions.DependencyInjection;
using RabbitRPC.ServiceHost.Filters;
using RabbitRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class, AllowMultiple=false)]
    public class AuthorizeAttribute : ActionFilterAttribute
    {
        public string? UserName { get; set; }

        public override void OnActionExecuting(IActionExecutingContext context)
        {
            if(context.CallContext.RequestProperties.TryGetHeader("X-Auth-UserId", out var userName))
            {
                if(UserName == null || userName==UserName)
                {
                    return;
                }
            }

            context.Exception = new UnauthorizedAccessException();
        }
    }
}
