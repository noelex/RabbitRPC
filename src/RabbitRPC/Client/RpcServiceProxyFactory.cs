using Microsoft.Extensions.DependencyInjection;
using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RabbitRPC.Client
{
    public static class RpcServiceProxyFactory
    {
        public static T CreateProxy<T>(IServiceProvider serviceProvider) where T : IRabbitService
        {
            var proxy=DispatchProxy.Create<T, RpcServiceProxy>();
            (proxy as RpcServiceProxy)!.Initialize(
                serviceProvider.GetRequiredService<IRabbitMQConnectionProvider>(), serviceProvider.GetRequiredService<IMessageSerializationProvider>());

            return proxy;
        }

        public static T CreateProxy<T>(IRabbitMQConnectionProvider connectionProvider, IMessageSerializationProvider messageSerializationProvider) where T : IRabbitService
        {
            var proxy = DispatchProxy.Create<T, RpcServiceProxy>();
            (proxy as RpcServiceProxy)!.Initialize(connectionProvider, messageSerializationProvider);

            return proxy;
        }
    }
}
