using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitRPC.Client.Filters;
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
    internal class RabbitServiceClient : DispatchProxy
    {
        private const string DefaultManagementExchangeName = "RabbitRPC";

        private IModel? _channel;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<BasicDeliverEventArgs>> _requests = new ConcurrentDictionary<string, TaskCompletionSource<BasicDeliverEventArgs>>();
        private IMessageBodyFactory? _messageBodyFactory;
        private IMessageBodySerializer? _messageBodySerializer;
        private RabbitServiceClientOptions? _options;
        private string? _responseQueue;
        private bool _isInitialized = false;
        private ILogger? _logger;

        private TimeSpan _defaultTimeout = TimeSpan.FromMinutes(2);

        public void Initialize(ILoggerFactory loggerFactory, IRabbitMQConnectionProvider connectionProvider, IMessageSerializationProvider messageSerializationProvider, RabbitServiceClientOptions options)
        {
            _logger = loggerFactory.CreateLogger(LoggerCategory.Proxy);
            _options = options;
            _messageBodyFactory = messageSerializationProvider.CreateMessageBodyFactory();
            _messageBodySerializer = messageSerializationProvider.CreateMessageBodySerializer();

            _ = ConnectLoopAsync(connectionProvider);
        }

        private async Task ConnectLoopAsync(IRabbitMQConnectionProvider connectionProvider)
        {
            while (true)
            {
                try
                {
                    _channel = connectionProvider.CreateConnection().CreateModel();
                    _responseQueue = _channel.QueueDeclare().QueueName;

                    _channel.ExchangeDeclare(DefaultManagementExchangeName, "fanout");

                    var consumer = new AsyncEventingBasicConsumer(_channel);
                    consumer.Received += (model, ea) =>
                    {
                        if (_requests.TryGetValue(ea.BasicProperties.CorrelationId, out var tcs))
                        {
                            tcs.TrySetResult(ea);
                        }

                        return Task.CompletedTask;
                    };
                    _channel.BasicConsume(_responseQueue, true, consumer);
                    _channel.BasicReturn += OnBasicReturn;

                    _isInitialized = true;
                    break;
                }
                catch (Exception ex)
                {
                    _logger!.LogError($"Failed to initialize service proxy: {ex.Message}");
                    await Task.Delay(5000);
                }
                break;
            }
        }

        private void OnBasicReturn(object sender, BasicReturnEventArgs e)
        {
            if (_requests.TryGetValue(e.BasicProperties.CorrelationId, out var task))
            {
                task.TrySetException(new RabbitRpcClientException($"No available replica found for service '{e.RoutingKey}'."));
            }
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException($"Failed to invoke proxy method '{targetMethod.DeclaringType.FullName}.{targetMethod.Name}'. Proxy is not initialized yet.");
            }

            if (!typeof(Task).IsAssignableFrom(targetMethod.ReturnType))
            {
                throw new NotSupportedException($"Action method '{targetMethod.Name}' define in '{targetMethod.DeclaringType.FullName}' does not return a Task or a Task<T>.");
            }

            // Prepare request context
            var correlationId = Guid.NewGuid().ToString();
            var context = new RequestContext(targetMethod, args, correlationId, _responseQueue!);

            // Prepare cancellation token
            var parentCancellationToken = args.Length > 0 &&
                args.Last() is CancellationToken token ? token : CancellationToken.None;
            var cancellableTokenSource = CancellationTokenSource.CreateLinkedTokenSource(parentCancellationToken);

            // Create filters
            var filters = _options!.Filters.OrderBy(x => x.Order)
                .Select(x => x.Filter is IProxyFilterFactory fac ? fac.CreateInstance() : x.Filter).ToArray();

            // Prepare request header and body
            var header = _channel!.CreateBasicProperties();
            var request = _messageBodyFactory!.CreateRequest(targetMethod.DeclaringType.FullName, targetMethod.Name, args.Length, null);
            var prepareContext = new PrepareRequestContext(context, header, request);
            filters.Invoke<IPrepareRequestFilter>(x => x.OnPrepareRequest(prepareContext));

            // Send cancellation signal to service is parent cancellation token is canceled
            var reg = cancellableTokenSource.Token.Register(() =>
            {
                var header = _channel!.CreateBasicProperties();

                header.CorrelationId = correlationId;
                header.Type = "Cancellation";
                _channel.BasicPublish(DefaultManagementExchangeName, context.ServiceName, basicProperties: header);
            });

            var callContext = new CallContext
            {
                CorrelationId = correlationId,
                CancellationRegistration = reg,
                CanncellationException = new OperationCanceledException(),
                CancellationTokenSource = cancellableTokenSource
            };

            // Create a recursive filter chain to process the response and invoke request filters.
            var executedContext = new RequestCompletedContext(context);
            var next = new RequestExecutionDelegate(InvokeRequestAsync);

            foreach (var filter in filters.OfType<IAsyncRequestFilter>().Reverse())
            {
                var nextAction = next;
                next = new RequestExecutionDelegate(async () =>
                {
                    if (!executedContext.Canceled)
                    {
                        await filter.OnRequestInvocationAsync(context, nextAction);
                    }

                    return executedContext;
                });
            }

            // Create a Task to wait for the request to complete and extract the result
            if (context.ReturnType == typeof(void))
            {
                return WaitAsync(next, executedContext);
            }
            else
            {
                var returnValueType = context.ReturnType;
                var invoke = typeof(RabbitServiceClient).GetMethod(nameof(WaitForResultAsync), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(returnValueType);
                return invoke.Invoke(this, new object[] { next, executedContext });
            }

            async Task<IRequestCompletedContext> InvokeRequestAsync()
            {
                filters.Not<IAsyncRequestFilter>().Invoke<IRequestFilter>(x => x.OnRequestStarting(context));

                using var timeoutCancellationToken = new CancellationTokenSource(_defaultTimeout);
                using var ctr = timeoutCancellationToken.Token.Register(() =>
                {
                    if (_requests.TryRemove(callContext.CorrelationId, out var tcs))
                    {
                        executedContext.Status = RequestStatus.TimedOut;

                        tcs.TrySetException(new TimeoutException("Timed out while waiting for RPC service to respond."));
                        callContext.CancellationTokenSource.Cancel();
                    };
                });

                using (callContext.CancellationRegistration)
                {
                    try
                    {
                        // Serialize request
                        var data = _messageBodySerializer!.Serialize(request);
                        var tcs = new TaskCompletionSource<BasicDeliverEventArgs>();

                        // Send request
                        _requests.TryAdd(correlationId, tcs);
                        _channel.BasicPublish("", context.RequestQueueName, true, header, data);

                        var ea = await tcs.Task;
                        var resp = _messageBodySerializer.DeserializeResponse(ea.Body);

                        var responseContext = new ResponseReceivedContext(context, ea.BasicProperties, ea.Body, resp);
                        filters.Invoke<IResponseReceivedFilter>(x => x.OnResponseReceived(responseContext));

                        if (resp.IsCancelled)
                        {
                            executedContext.Status = RequestStatus.Aborted;
                            executedContext.Exception = new OperationCanceledException(callContext.CancellationRegistration.Token);
                        }
                        else if (resp.Exception != null)
                        {
                            executedContext.Status = RequestStatus.ServerError;
                            executedContext.Exception = new AggregateException(resp.Exception);
                        }
                        else
                        {
                            executedContext.Status = RequestStatus.Sucess;
                            if (executedContext.ActionMethod.ReturnType.IsGenericType)
                            {
                                var returnType = context.ActionMethod.ReturnType.GetGenericArguments()[0];
                                executedContext.Result = resp.GetReturnValue(returnType);
                            }
                        }
                    }
                    catch (TimeoutException timeout)
                    {
                        executedContext.Status = RequestStatus.TimedOut;
                        executedContext.Exception = timeout;
                    }
                    catch (Exception ex)
                    {
                        executedContext.Status = RequestStatus.ClientError;
                        executedContext.Exception = ex;
                    }
                }

                filters.Not<IAsyncRequestFilter>().Invoke<IRequestFilter>(x => x.OnRequestCompleted(executedContext));
                return executedContext;
            }
        }

        private async Task WaitAsync(RequestExecutionDelegate next, RequestCompletedContext context)
        {
            await next();
            if (context.Exception != null && !context.ExceptionHandled)
            {
                ExceptionDispatchInfo.Throw(context.Exception);
            }
        }

        private async Task<T> WaitForResultAsync<T>(RequestExecutionDelegate next, RequestCompletedContext context)
        {
            await WaitAsync(next, context);
            return (T)context.Result!;
        }

        internal struct CallContext
        {
            public string CorrelationId { get; set; }

            public CancellationTokenRegistration CancellationRegistration { get; set; }

            public CancellationTokenSource CancellationTokenSource { get; set; }

            public Exception CanncellationException { get; set; }
        }
    }
}