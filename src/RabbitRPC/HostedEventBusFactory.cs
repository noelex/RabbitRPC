using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly ILoggerFactory _loggerFactory;

        public HostedEventBusFactory(ILoggerFactory loggerFactory, IRabbitMQConnectionProvider connectionProvider, IMessageSerializationProvider serializationProvider)
            => (_loggerFactory, _connectionProvider, _serializationProvider) = (loggerFactory, connectionProvider, serializationProvider);

        public IHostedEventBus CreateHostedEventBus(string exchangeName, bool durable = false, bool autoDelete = false)
        {
            return new RabbitEventBus(_loggerFactory, _connectionProvider, _serializationProvider, exchangeName, durable, autoDelete);
        }
    }
}
