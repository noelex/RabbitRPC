using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitRPC.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.WorkQueues
{
    internal class WorkItemSubject<T> : RabbitEventSubject<WorkItem<T>>
    {
        public WorkItemSubject(string exchangeName, string routingKey, IMessageBodySerializer serializer, string queueName)
            :base(exchangeName, routingKey, serializer, queueName, false)
        {
        }

        protected override WorkItem<T> DeserializeMessage(IMessageBodySerializer serializer, Type type, IModel channel, BasicDeliverEventArgs ea)
        {
            return new WorkItem<T>(serializer.Deserialize<T>(ea.Body, type)) { Channel = channel, Data = ea };
        }
    }
}
