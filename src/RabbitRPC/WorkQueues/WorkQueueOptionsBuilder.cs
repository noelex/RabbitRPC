using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace RabbitRPC.WorkQueues
{
    public class WorkQueueOptions
    {
        public WorkQueueOptions(IList<Type> workItemTypes ) => WorkItemTypes = workItemTypes;

        public IList<Type> WorkItemTypes { get; }
    }

    public class WorkQueueOptionsBuilder
    {
        private readonly IServiceCollection _services;
        private readonly List<Type> _workItemTypes = new List<Type>();


        internal WorkQueueOptionsBuilder(IServiceCollection services)
        {
            _services = services;
        }

        public WorkQueueOptionsBuilder AddHandler<TWorkItem, TWorkItemHandler>(Action<WorkItemHandlerOptions<TWorkItem>>? configure=null, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where TWorkItemHandler: class, IWorkItemHandler<TWorkItem>
        {
            _services.Add(new ServiceDescriptor(typeof(IWorkItemHandler<TWorkItem>), typeof(TWorkItemHandler), serviceLifetime));

            var options = new WorkItemHandlerOptions<TWorkItem>();
            configure?.Invoke(options);
            _services.AddSingleton(options);

            _workItemTypes.Add(typeof(TWorkItem));
            return this;
        }

        public WorkQueueOptions Build() => new WorkQueueOptions(_workItemTypes);

    }
}
