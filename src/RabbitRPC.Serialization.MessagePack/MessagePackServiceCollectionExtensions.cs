using RabbitRPC.Serialization;
using RabbitRPC.Serialization.MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MessagePackServiceCollectionExtensions
    {
        public static IServiceCollection AddMessagePackSerializationProvider(this IServiceCollection services)
        {
            return services.AddSingleton<IMessageSerializationProvider, MessagePackMessageSerializationProvider>();
        }
    }
}
