using System.Collections.Generic;

namespace RabbitRPC.ServiceHost.Filters
{
    internal class ActionExecutingContext : FilterContext, IActionExecutingContext
    {
        public ActionExecutingContext(ActionContext actionContext, IList<IFilterMetadata> filters,  IDictionary<string, object?> args, IRabbitService service)
            : base(actionContext, filters)
        {
            ActionArguments = args;
            Service = service;
        }

        public IDictionary<string, object?> ActionArguments { get; }

        public virtual IRabbitService Service { get; }

        public virtual object? Result { get; set; }
    }
}
