using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost.Filters
{
    internal abstract class FilterContext : ActionContext, IFilterContext
    {
        public FilterContext(ActionContext actionContext, IList<IFilterMetadata> filters)
            :base(actionContext)
        {
            Filters = filters;
        }

        public virtual IList<IFilterMetadata> Filters { get; set; }
    }
}
