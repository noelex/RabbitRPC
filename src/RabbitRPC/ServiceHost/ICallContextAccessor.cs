using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.ServiceHost
{
    /// <summary>
    /// Provides access to the current <see cref="ICallContext"/>, if one is available.
    /// </summary>
    /// <remarks>
    /// This interface should be used with caution. It relies on <see cref="System.Threading.AsyncLocal{T}" /> which can have a negative performance impact on async calls.
    /// It also creates a dependency on "ambient state" which can make testing more difficult.
    /// </remarks>
    public interface ICallContextAccessor
    {
        /// <summary>
        /// Gets or sets the current <see cref="ICallContext"/>. Returns <see langword="null" /> if there is no active <see cref="ICallContext" />.
        /// </summary>
        ICallContext? CallContext { get; set; }
    }
}
