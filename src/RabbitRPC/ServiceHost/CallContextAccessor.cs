using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RabbitRPC.ServiceHost
{
    /// <summary>
    /// Provides an implementation of <see cref="ICallContextAccessor" /> based on the current execution context.
    /// </summary>
    public class CallContextAccessor: ICallContextAccessor
    {
        private static AsyncLocal<CallContextHolder> _callContextCurrent = new AsyncLocal<CallContextHolder>();

        public ICallContext? CallContext
        {
            get
            {
                return _callContextCurrent.Value?.Context;
            }
            set
            {
                var holder = _callContextCurrent.Value;
                if (holder != null)
                {
                    // Clear current HttpContext trapped in the AsyncLocals, as its done.
                    holder.Context = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the HttpContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    _callContextCurrent.Value = new CallContextHolder { Context = value };
                }
            }
        }

        private class CallContextHolder
        {

            public ICallContext? Context;
        }
    }
}
