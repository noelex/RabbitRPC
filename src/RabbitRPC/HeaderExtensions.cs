using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RabbitRPC
{
    public static class HeaderExtensions
    {
        public static void SetHeader(this IBasicProperties prop, string key, string value)
        {
            if (!prop.IsHeadersPresent())
            {
                prop.Headers = new Dictionary<string, object>();
            }

            prop.Headers[key] = value;
        }

        public static void ClearHeader(this IBasicProperties prop, string key)
        {
            if (prop.IsHeadersPresent())
            {
                prop.Headers.Remove(key);
            }
        }

        public static bool TryGetHeader(this IBasicProperties prop, string key,[NotNullWhen(true)] out string? value)
        {
            if (prop.IsHeadersPresent())
            {
                if( prop.Headers.TryGetValue(key, out var bytes))
                {
                    value = Encoding.UTF8.GetString((byte[])bytes);
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}
