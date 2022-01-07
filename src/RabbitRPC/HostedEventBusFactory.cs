using Microsoft.Extensions.DependencyInjection;
using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC
{
    public class HostedEventBusFactory : IHostedEventBusFactory
    {
        private readonly IRabbitMQConnectionProvider _connectionProvider;
        private readonly IMessageSerializationProvider _serializationProvider;

        public HostedEventBusFactory(IRabbitMQConnectionProvider connectionProvider, IMessageSerializationProvider serializationProvider)
            => (_connectionProvider, _serializationProvider) = (connectionProvider, serializationProvider);

        public IHostedEventBus CreateHostedEventBus(string exchangeName, bool durable = false, bool autoDelete = false)
        {
            return new RabbitEventBus(_connectionProvider, _serializationProvider, exchangeName, durable, autoDelete);
        }
    }
}
