using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitRPC.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RabbitRPC
{
    internal interface IRabbitEventSubject: IDisposable
    {
        void OnConnected(IModel model);
    }

    public class RabbitEventSubject<T> : IObservable<T>, IRabbitEventSubject
    {
        private static readonly ConcurrentDictionary<string, Type> _eventTypeMap = new ConcurrentDictionary<string, Type>();

        private readonly ConcurrentDictionary<Guid, IObserver<T>> _observers = new ConcurrentDictionary<Guid, IObserver<T>>();
        private readonly string _exchangeName, _routingKey;
        private readonly IMessageBodySerializer _bodySerializer;
        private readonly bool _autoAck;

        private readonly string? _queueName;

        public RabbitEventSubject(string exchangeName, string routingKey, IMessageBodySerializer serializer,string? queueName = null, bool autoAck=true)
        {
            (_exchangeName, _routingKey) = (exchangeName, routingKey);
            _bodySerializer = serializer;
            _autoAck = autoAck;
            _queueName = queueName;
        }

        private bool TryResolve(string routingKey, out Type type)
        {
            if (_eventTypeMap.TryGetValue(routingKey, out type))
            {
                return true;
            }
            else
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetType(routingKey);

                    if (type != null)
                    {
                        _eventTypeMap.TryAdd(routingKey, type);
                        return true;
                    }
                }

                return false;
            }
        }

        protected virtual T DeserializeMessage(IMessageBodySerializer serializer, Type type, IModel channel, BasicDeliverEventArgs ea)
        {
            return _bodySerializer.Deserialize<T>(ea.Body, type);
        }

        public void OnConnected(IModel model)
        {
            var queue = _queueName is null ? model.QueueDeclare(exclusive: true).QueueName : model.QueueDeclare(_queueName, true, false, false).QueueName;
            model.QueueBind(queue, _exchangeName, _routingKey);

            var consumer = new AsyncEventingBasicConsumer(model);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    if (TryResolve(ea.RoutingKey, out var type))
                    {
                        OnNext(DeserializeMessage(_bodySerializer, type,((AsyncEventingBasicConsumer)model).Model, ea));
                    }
                    else
                    {
                        throw new TypeLoadException($"Type '{ea.RoutingKey}' is not found.");
                    }
                }
                catch (Exception e)
                {
                    e.Data["model"] = model;
                    e.Data["ea"] = ea;

                    OnError(e);
                }

                return System.Threading.Tasks.Task.CompletedTask;
            };
            model.BasicConsume(queue, _autoAck, consumer);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var subscriptionId = Guid.NewGuid();
            _observers[subscriptionId] = observer;

            return new DelegateDisposable(() => _observers.TryRemove(subscriptionId,out _));
        }

        public void OnNext(T next)
        {
            foreach (var observer in _observers.Values)
            {
                observer.OnNext(next);
            }
        }

        public void OnError(Exception error)
        {
            foreach (var observer in _observers.Values)
            {
                observer.OnError(error);
            }
        }

        public void OnCompleted()
        {
            foreach (var observer in _observers.Values)
            {
                observer.OnCompleted();
            }
        }

        public void Dispose()
        {
            OnCompleted();
            _observers.Clear();
        }
    }

    internal class DelegateDisposable : IDisposable
    {
        private readonly Action _onDispose;

        public DelegateDisposable(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose();
        }
    }
}
