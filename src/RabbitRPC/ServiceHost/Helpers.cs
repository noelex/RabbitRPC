using Microsoft.Extensions.Logging;
using RabbitRPC.ServiceHost.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.ServiceHost
{
    internal static class Helpers
    {
        public static void Invoke<T>(this IEnumerable<IFilterMetadata> filters, Action<T> action, ILogger logger, string filterName)
        {
            var sw = new Stopwatch();
            foreach(var filter in filters.OfType<T>())
            {
                sw.Restart();
                action(filter);
                sw.Stop();

                logger.LogTrace($"Finished invoking {filterName} on filter {filter!.GetType().Name} in {sw.Elapsed.TotalMilliseconds:F4}ms.");
            }
        }

        public static IEnumerable<IFilterMetadata> Not<T>(this IEnumerable<IFilterMetadata> filters)
            => filters.Where(x => !(x is T));

        public static async Task WaitAsync(this WaitHandle waitHandle, CancellationToken cancellationToken)
        {
            if (waitHandle == null)
                throw new ArgumentNullException(nameof(waitHandle));

            var tcs = new TaskCompletionSource<bool>();

            RegisteredWaitHandle registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                waitHandle,
                callBack: (state, timedOut) => { tcs.TrySetResult(true); },
                state: null,
                millisecondsTimeOutInterval: -1,
                executeOnlyOnce: true);

            using var ct=cancellationToken.Register(() =>
            {
                registeredWaitHandle.Unregister(null);
                tcs.TrySetCanceled();
            });

            await tcs.Task;

            registeredWaitHandle.Unregister(null);
        }
    }
}
