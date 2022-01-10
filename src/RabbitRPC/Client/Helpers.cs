using RabbitRPC.Client.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitRPC.Client
{
    internal static class Helpers
    {
        public static void Invoke<T>(this IEnumerable<IProxyFilterMetadata> filters, Action<T> action)
        {
            foreach (var filter in filters.OfType<T>())
            {
                action(filter);
            }
        }

        public static IEnumerable<IProxyFilterMetadata> Not<T>(this IEnumerable<IProxyFilterMetadata> filters)
            => filters.Where(x => !(x is T));
    }
}
