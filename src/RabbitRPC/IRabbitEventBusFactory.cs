using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC
{
    /// <summary>
    /// Expose factory method to create <see cref="IHostedEventBus"/> instances.
    /// </summary>
    public interface IHostedEventBusFactory
    {
        IHostedEventBus CreateHostedEventBus(string exchangeName, bool durable=false, bool autoDelete=false);
    }
}
