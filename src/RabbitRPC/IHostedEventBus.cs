using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC
{
    /// <summary>
    /// Represent a <see cref="IRabbitEventBus"/> which can be hosted by a service host.
    /// </summary>
    public interface IHostedEventBus : IRabbitEventBus, IHostedService
    {
    }
}
