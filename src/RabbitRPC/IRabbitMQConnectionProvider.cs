using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC
{
    public interface IRabbitMQConnectionProvider
    {
        IConnection CreateConnection();
    }
}
