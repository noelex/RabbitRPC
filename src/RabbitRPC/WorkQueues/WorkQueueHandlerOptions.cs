using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.WorkQueues
{
    public class WorkItemHandlerOptions
    {
        /// <summary>
        /// Gets or sets the degree of parallelism to use when processing work items.
        /// Degree of parallelism is the maximum number of batches of can be concurrently processed by <see cref="IWorkItemHandler{T}"/>.
        /// </summary>
        /// <remarks>The default value is Math.Min(System.Environment.ProcessorCount, MAX_SUPPORTED_DOP) where MAX_SUPPORTED_DOP is 512.</remarks>
        public int DegreeOfParallelism { get; set; } = Math.Min(Environment.ProcessorCount, 512);

        /// <summary>
        /// Gets or sets a value indicating how should the dispatcher dispatch parallel work item batches.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="BatchConcurrencyMode.Isolated"/>.
        /// This option will only take effect when parallel processing is enabled (i.e. <see cref="DegreeOfParallelism"/> is set to a value larger than 1).
        /// </remarks>
        public BatchConcurrencyMode ConcurrencyMode { get; set; } = BatchConcurrencyMode.Isolated;

        /// <summary>
        /// The maximum number of work items in a batch.
        /// This option also determines the maximum number of work items allowed to be dequeued in a single batch session, which is <see cref="BatchSize"/> * <see cref="DegreeOfParallelism"/>.
        /// </summary>
        /// <remarks>
        /// The default value is 32.
        /// </remarks>
        public int BatchSize { get; set; } = 32;

        /// <summary>
        /// Maximum time (in milliseconds) to wait until a batch gets filled.
        /// </summary>
        /// <remarks>
        /// The default value is 0.
        /// </remarks>
        public int BatchTimeout { get; set; } = 0;

        /// <summary>
        /// The maximum number of work items allowed to be buffered in memory before they are dispatched to the handler.
        /// </summary>
        /// <remarks>
        /// The default value is 1024.
        /// </remarks>
        public int BufferSize { get; set; } = 1024;
    }

    /// <summary>
    /// Specify how should the dipatcher disptach parallel work item batches.
    /// </summary>
    public enum BatchConcurrencyMode
    {
        /// <summary>
        /// Batches in a parallel batch will be processed concurrently in their own batch sessions.
        /// </summary>
        /// <remarks>
        /// Please note that using <see cref="Isolated"/> mode does not neccesarily mean that multiple <see cref="IWorkItemHandler{T}"/> instances will be created.
        /// If a <see cref="IWorkItemHandler{T}"/> is registered as a <see cref="ServiceLifetime.Singleton"/> service, it still need to guarentee thread safty since
        /// all batch sessions will be sharing a single <see cref="IWorkItemHandler{T}"/> instance.
        /// </remarks>
        Isolated,

        /// <summary>
        /// Batches in a parallel batch will be processed concurrently in a single shared batch session. 
        /// </summary>
        /// <remarks>
        /// This mode should be used only when the <see cref="IWorkItemHandler{T}"/> guarentees thread safty.
        /// </remarks>
        Shared
    }

    public sealed class WorkItemHandlerOptions<T> : WorkItemHandlerOptions { }
}
