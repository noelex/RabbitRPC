using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.WorkQueues
{
    public interface IWorkItemHandler<T>
    {
        Task ProcessAsync(ReadOnlyMemory<WorkItem<T>> items, CancellationToken cancellationToken);
    }
}
