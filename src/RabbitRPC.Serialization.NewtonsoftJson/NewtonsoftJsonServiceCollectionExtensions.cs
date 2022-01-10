using RabbitRPC.Serialization;
using RabbitRPC.Serialization.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NewtonsoftJsonServiceCollectionExtensions
    {
        public static IServiceCollection AddJsonSerializationProvider(this IServiceCollection services)
        {
            return services.AddSingleton<IMessageSerializationProvider, JsonMessageSerializationProvider>();
        }
    }
}
