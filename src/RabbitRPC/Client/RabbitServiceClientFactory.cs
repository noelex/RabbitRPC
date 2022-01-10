using Microsoft.Extensions.DependencyInjection;
using RabbitRPC.Client.Filters.Internal;
using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RabbitRPC.Client
{
    public static class RabbitServiceClientFactory
    {
        public static T CreateProxy<T>(IServiceProvider serviceProvider, Action<RabbitServiceClientOptionBuilder>? configure = null) where T : IRabbitService
            => CreateProxy<T>(serviceProvider.GetRequiredService<IRabbitMQConnectionProvider>(), serviceProvider.GetRequiredService<IMessageSerializationProvider>(), configure);

        public static T CreateProxy<T>(IRabbitMQConnectionProvider connectionProvider,
            IMessageSerializationProvider messageSerializationProvider,
            Action<RabbitServiceClientOptionBuilder>? configure = null) where T : IRabbitService
        {
            var builder = new RabbitServiceClientOptionBuilder(typeof(T));
            builder.AddFilter(new DefaultPrepareRequestFilter(), int.MinValue);

            configure?.Invoke(builder);
            var options = builder.Build();

            var proxy = DispatchProxy.Create<T, RabbitServiceClient>();
            (proxy as RabbitServiceClient)!.Initialize(connectionProvider, messageSerializationProvider, options);

            return proxy;
        }
    }
}
