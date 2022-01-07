using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.WorkQueues
{
    public class RabbitWorkQueue : IWorkQueue, IHostedService
    {
        private const string DefaultExchangeName = "RabbitRPC.WorkQueue";

        private readonly IHostedEventBus _eventBus;
        private readonly WorkQueueOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, HandlerSpec> _handlers;
        private readonly List<IWorkItemDispatcher> _dispatchers = new List<IWorkItemDispatcher>();

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public RabbitWorkQueue(WorkQueueOptions options, IServiceProvider serviceProvider, IHostedEventBusFactory hostedEventBusFactory)
        {
            _options = options;
            _serviceProvider = serviceProvider;
            _eventBus = hostedEventBusFactory.CreateHostedEventBus(DefaultExchangeName, true);

            _handlers = _options.WorkItemTypes.ToDictionary(x => x.FullName,
                x => new HandlerSpec(x, typeof(IWorkItemHandler<>).MakeGenericType(x),
                (WorkItemHandlerOptions)serviceProvider.GetRequiredService(typeof(WorkItemHandlerOptions<>).MakeGenericType(x))));
        }

        public void Post<T>(T workItem)
        {
            _eventBus.Publish(workItem);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _eventBus.StartAsync(cancellationToken);

            foreach(var (_,spec) in _handlers)
            {
                var disp = (IWorkItemDispatcher)Activator.CreateInstance(
                    typeof(WorkItemDispatcher<>).MakeGenericType(spec.WorkItemType), new object[] { _serviceProvider, _eventBus, spec });

                _dispatchers.Add(disp);
            }

            _ = Task.WhenAll(_dispatchers.Select(x => x.RunAsync(_cts.Token)));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            foreach(var disp in _dispatchers)
            {
                disp.Dispose();
            }

            return _eventBus.StopAsync(cancellationToken);
        }
    }
}
