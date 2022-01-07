using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitRPC.Serialization;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("RabbitRPC.ServiceHost")]
[assembly: InternalsVisibleTo("RabbitRPC.Client")]

namespace RabbitRPC
{
    internal class RabbitEventBus : IHostedEventBus
    {
        private const string DefaultExchangeName = "RabbitRPC.EventBus";

        private readonly ConcurrentDictionary<string, IRabbitEventSubject> _subjects = new ConcurrentDictionary<string, IRabbitEventSubject>();
        private readonly IMessageBodySerializer _bodySerializer;
        private readonly IRabbitMQConnectionProvider _connectionProvider;
        private readonly string _exchangeName;
        private readonly bool _durable=false,_autoDelete=false;

        private IModel? _model;
        private IBasicProperties? _defaultHeader;

        public RabbitEventBus(IRabbitMQConnectionProvider connectionProvider, IMessageSerializationProvider messageSerializationProvider)
            :this(connectionProvider, messageSerializationProvider, DefaultExchangeName, false, false)
        {
        }

        internal RabbitEventBus(IRabbitMQConnectionProvider connectionProvider,
            IMessageSerializationProvider messageSerializationProvider, string exchangeName, bool durable = false, bool autoDelete = false)
        {
            _bodySerializer = messageSerializationProvider.CreateMessageBodySerializer();
            _connectionProvider = connectionProvider;

            _exchangeName = exchangeName;
            _durable = durable;
            _autoDelete = autoDelete;
        }

        public IObservable<T> Observe<T>()
        {
            var key = typeof(T).FullName;
            var sub = (RabbitEventSubject<T>)_subjects.GetOrAdd(key, x => new RabbitEventSubject<T>(_exchangeName, key, _bodySerializer));

            if(_model!=null) sub.OnConnected(_model);

            return sub;
        }

        public IObservable<T> Observe<T>(string routingKey)
        {
            return Observe(routingKey, (ex, k, s) => new RabbitEventSubject<T>(ex, k, s));
        }

        public IObservable<T> Observe<T>(string routingKey, Func<string,string, IMessageBodySerializer, RabbitEventSubject<T>> subjectFactory)
        {
            var sub = (RabbitEventSubject<T>)_subjects.GetOrAdd(routingKey,
                x => subjectFactory(_exchangeName, routingKey, _bodySerializer));

            if (_model != null) sub.OnConnected(_model);

            return sub;
        }

        public void Publish<T>(T @event)
        {
            if (_model is null || _defaultHeader is null) throw new InvalidOperationException("RabbitEventBus is not started yet.");

            var data = _bodySerializer.Serialize(@event);
            _model.BasicPublish(_exchangeName, typeof(T).FullName, _defaultHeader, data);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var conn = _connectionProvider.CreateConnection();
            _model = conn.CreateModel();
            _defaultHeader = _model.CreateBasicProperties();

            _model.ExchangeDeclare(_exchangeName, "topic", _durable, _autoDelete);

            foreach(var (_,sub) in _subjects)
            {
                sub.OnConnected(_model);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach(var (_,v) in _subjects)
            {
                v.Dispose();
            }

            _model?.Dispose();
            return Task.CompletedTask;
        }
    }
}
