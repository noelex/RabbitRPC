using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.States
{
    /// <summary>
    /// Specifies the isolation level of a transaction.
    /// </summary>
    public enum IsolationLevel
    {
        /// <summary>
        /// The <see cref="IsolationLevel"/> to use is determined by the underlying provider.
        /// </summary>
        Default = -1,

        /// <summary>
        /// Volatile data can be read but not modified, and no new data can be added during the transaction.
        /// </summary>
        Serializable = 0,

        /// <summary>
        /// Volatile data can be read but not modified during the transaction. New data can be added during the transaction.
        /// </summary>
        RepeatableRead = 1,

        /// <summary>
        /// Volatile data cannot be read during the transaction, but can be modified.
        /// </summary>
        ReadCommitted = 2,

        /// <summary>
        /// Volatile data can be read and modified during the transaction.
        /// </summary>
        ReadUncommited = 3,

        /// <summary>
        /// Volatile data can be read. Before a transaction modifies data, it verifies if another transaction has changed the data after it was initially read.
        /// If the data has been updated, an error is raised. This allows a transaction to get to the previously committed value of the data.
        /// </summary>
        Snapshot = 4,

        /// <summary>
        /// The pending changes from more highly isolated transactions cannot be overwritten.
        /// </summary>
        Chaos = 5
    }
}
