using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitRPC.Serialization;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.Client
{
    internal class RpcServiceProxy : DispatchProxy
    {
        private const string DefaultManagementExchangeName = "RabbitRPC";

        private IModel? _channel;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<IResponseMessageBody>> _requests = new ConcurrentDictionary<string, TaskCompletionSource<IResponseMessageBody>>();
        private IMessageBodyFactory? _messageBodyFactory;
        private IMessageBodySerializer? _messageBodySerializer;
        private string? _responseQueue;
        private bool _isInitialized = false;

        private TimeSpan _defaultTimeout = TimeSpan.FromMinutes(2);

        public void Initialize(IRabbitMQConnectionProvider connectionProvider, IMessageSerializationProvider messageSerializationProvider)
        {
            _messageBodyFactory = messageSerializationProvider.CreateMessageBodyFactory();
            _messageBodySerializer = messageSerializationProvider.CreateMessageBodySerializer();

            _channel = connectionProvider.CreateConnection().CreateModel();
            _responseQueue = _channel.QueueDeclare().QueueName;

            _channel.ExchangeDeclare(DefaultManagementExchangeName, "fanout");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                if (_requests.TryGetValue(ea.BasicProperties.CorrelationId, out var tcs))
                {
                    try
                    {
                        var responseBody = _messageBodySerializer.DeserializeResponse(ea.Body);
                        tcs.TrySetResult(responseBody);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                }

                return Task.CompletedTask;
            };
            _channel.BasicConsume(_responseQueue, true, consumer);
            _channel.BasicReturn += OnBasicReturn;

            _isInitialized = true;
        }

        private void OnBasicReturn(object sender, BasicReturnEventArgs e)
        {
            if (_requests.TryGetValue(e.BasicProperties.CorrelationId, out var task))
            {
                task.TrySetException(new TargetException($"No available replica found for service '{e.RoutingKey}'."));
            }
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException($"Failed to invoke proxy method '{targetMethod.DeclaringType.FullName}.{targetMethod.Name}'. Proxy is not initialized yet.");
            }

            var serviceName= targetMethod.DeclaringType.GetCustomAttribute<RabbitServiceAttribute>()?.Name ?? targetMethod.DeclaringType.FullName;
            var actionName = targetMethod.GetCustomAttribute<ActionAttribute>()?.Name ?? targetMethod.Name;

            var queueName = "RabbitRPC:" + serviceName;

            var request = _messageBodyFactory!.CreateRequest(targetMethod.DeclaringType.FullName, targetMethod.Name, args.Length, null);
            var parameters = targetMethod.GetParameters();

            var cancellationToken = args.Length > 0 &&
                args.Last() is CancellationToken ? (CancellationToken)args.Last() : CancellationToken.None;

            var cancellableTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            for (var i = 0; i <= parameters.Length; i++)
            {
                if (parameters.Length - 1 == i && args[i] is CancellationToken)
                {
                    break;
                }
                request.SetParameter(i, parameters[i].Name, args[i]);
            }

            var correlationId = Guid.NewGuid().ToString();
            var data = _messageBodySerializer!.Serialize(request);
            var tcs = new TaskCompletionSource<IResponseMessageBody>();

            _requests.TryAdd(correlationId, tcs);

            var header = _channel!.CreateBasicProperties();
            header.CorrelationId = correlationId;
            header.ReplyTo = _responseQueue!;
            header.Type = actionName;
            _channel.BasicPublish("", queueName, true, header, data);

            var reg = cancellableTokenSource.Token.Register(() =>
            {
                var header = _channel!.CreateBasicProperties();

                header.CorrelationId = correlationId;
                header.Type = "Cancellation";
                _channel.BasicPublish(DefaultManagementExchangeName, serviceName, basicProperties: header);
            });

            var callContext = new CallContext
            {
                CorrelationId = correlationId,
                TaskCompletionSource = tcs,
                CancellationRegistration = reg,
                CanncellationException = new OperationCanceledException(),
                CancellationTokenSource = cancellableTokenSource
            };

            if (targetMethod.ReturnType == typeof(Task))
            {
                return InvokeAsync(callContext);
            }
            else
            {
                var returnValueType = targetMethod.ReturnType.GetGenericArguments()[0];
                var invoke = GetType().GetMethod(nameof(InvokeGenericAsync)).MakeGenericMethod(returnValueType);
                return invoke.Invoke(this, new object[] { callContext });
            }
        }

        private async Task InvokeAsync(CallContext ctx)
        {
            using var timeoutCancellationToken = new CancellationTokenSource(_defaultTimeout);
            using var ctr = timeoutCancellationToken.Token.Register(() =>
            {
                if (_requests.TryRemove(ctx.CorrelationId, out var tcs))
                {
                    tcs.TrySetException(new TimeoutException("Timed out while waiting for RPC service to respond."));
                    ctx.CancellationTokenSource.Cancel();
                };
            });

            using (ctx.CancellationRegistration)
            {
                var resp = await ctx.TaskCompletionSource.Task;
                if (resp.IsCancelled)
                {
                    throw new OperationCanceledException(ctx.CancellationRegistration.Token);
                }

                if (resp.Exception != null)
                {
                    throw new AggregateException(resp.Exception);
                }
            }
        }

        public async Task<T> InvokeGenericAsync<T>(CallContext ctx)
        {
            using var timeoutCancellationToken = new CancellationTokenSource(_defaultTimeout);
            using var ctr = timeoutCancellationToken.Token.Register(() =>
            {
                if (_requests.TryRemove(ctx.CorrelationId, out var tcs))
                {
                    tcs.TrySetException(new TimeoutException("Timed out while waiting for RPC service to respond."));
                    ctx.CancellationTokenSource.Cancel();
                };
            });

            using (ctx.CancellationRegistration)
            {
                var resp = await ctx.TaskCompletionSource.Task;
                if (resp.IsCancelled)
                {
                    throw new OperationCanceledException(ctx.CancellationRegistration.Token);
                }

                if (resp.Exception != null)
                {
                    throw new AggregateException(resp.Exception);
                }

                return (T)resp.GetReturnValue(typeof(T))!;
            }
        }

        internal struct CallContext
        {
            public string CorrelationId { get; set; }

            public TaskCompletionSource<IResponseMessageBody> TaskCompletionSource { get; set; }

            public CancellationTokenRegistration CancellationRegistration { get; set; }

            public CancellationTokenSource CancellationTokenSource { get; set; }

            public Exception CanncellationException { get; set; }
        }
    }
}