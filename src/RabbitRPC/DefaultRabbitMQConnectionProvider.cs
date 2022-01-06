using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC
{
    public class DefaultRabbitMQConnectionProvider : IRabbitMQConnectionProvider
    {
        private readonly IConnection _connection;
        private readonly IConnectionFactory _connectionFactory;

        public DefaultRabbitMQConnectionProvider(IConnectionFactory connectionFactory)
        {
            _connectionFactory= connectionFactory;
            _connection = connectionFactory.CreateConnection();
        }

        public IConnection CreateConnection() => _connection;
    }
}
