using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost.Filters
{
    /// <summary>
    /// An abstract context for filters.
    /// </summary>
    public interface IFilterContext : IActionContext
    {
        /// <summary>
        /// Gets all applicable <see cref="IFilterMetadata"/> implementations.
        /// </summary>
        IList<IFilterMetadata> Filters { get; }
    }
}
