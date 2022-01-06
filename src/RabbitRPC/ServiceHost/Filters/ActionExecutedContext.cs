using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;

namespace RabbitRPC.ServiceHost.Filters
{
    internal class ActionExecutedContext : FilterContext, IActionExecutedContext
    {
        public ActionExecutedContext(ActionContext actionContext, IList<IFilterMetadata> filters, IRabbitService service)
             : base(actionContext, filters)
        {
            Service = service;
        }

        public virtual bool Canceled { get; set; }

        public virtual bool ExceptionHandled { get; set; }

        public virtual IRabbitService Service { get; set; }

        public virtual Exception? Exception { get; set; }

        public virtual object? Result { get; set; }

        public virtual ExceptionHandlingStrategy UnhandledExceptionHandlingStrategy { get; set; }
    }
}
