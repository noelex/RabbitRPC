using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RabbitRPC
{
    public interface IRabbitEventBus
    {
        IObservable<T> Observe<T>();

        IObservable<T> Observe<T>(string routingKey);

        IObservable<T> Observe<T>(string routingKey, Func<string, string, IMessageBodySerializer, RabbitEventSubject<T>> subjectFactory);

        void Publish<T>(T @event);
    }
}
