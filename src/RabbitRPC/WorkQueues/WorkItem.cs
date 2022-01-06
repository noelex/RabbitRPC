using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.WorkQueues
{
    /// <summary>
    /// Represent a work item in distributed work queue.
    /// </summary>
    /// <typeparam name="T">Type of the work item.</typeparam>
    public class WorkItem<T>
    {
        /// <summary>
        /// Instantiate a new <see cref="WorkItem{T}"/> with specified actual work item object.
        /// </summary>
        /// <param name="val">The actual work item object to be wrapped.</param>
        public WorkItem(T val)
            => Value = val;

        /// <summary>
        /// Gets the underlying work item.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets or sets a value indicates that whether the handler has done processing the work item.
        /// </summary>
        /// <remarks>
        /// When this property is set to <see langword="true"/>, the dispatcher will send an acknowledgement to the message broker.
        /// Otherwise, the message will be requeued so that it can be retried processing later.
        /// </remarks>
        public bool IsDone { get; set; }

        internal BasicDeliverEventArgs Data { get; set; } = null!;

        internal IModel Channel { get; set; } = null!;
    }
}
