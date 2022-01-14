using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitRPC.Serialization;
using RabbitRPC.ServiceHost.Filters;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RabbitRPC.ServiceHost
{
    internal class RabbitServiceHost : IHostedService
    {
        private const string DefaultManagementExchangeName = "RabbitRPC";

        private readonly RabbitServiceHostOptions _options;
        private readonly IMessageBodyFactory _messageBodyFactory;
        private readonly IMessageBodySerializer _messageBodySerializer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly string _hostId = Guid.NewGuid().ToString();
        private readonly ICallContextAccessor _callContextAccessor;

        private readonly ConcurrentDictionary<string, CancellationTokenSource> _ongoingRequests = new ConcurrentDictionary<string, CancellationTokenSource>();
        private readonly Channel<RequestData> _requestChannel;
        private readonly CancellationTokenSource _stopSignal = new CancellationTokenSource();

        private IConnection? _connection;
        private IModel? _invokeChannel, _cancellationChannel;

        private IRabbitMQConnectionProvider _connectionProvider;

        public RabbitServiceHost(IOptions<RabbitServiceHostOptions> options, ILoggerFactory loggerFactory, ICallContextAccessor callContextAccessor,
            IRabbitMQConnectionProvider connectionProvider, IMessageSerializationProvider serializationProvider, IServiceProvider serviceProvider)
        {
            _logger = loggerFactory.CreateLogger(LoggerCategory.Hosting);

            _options = options.Value;
            _connectionProvider = connectionProvider;
            _serviceProvider = serviceProvider;
            _callContextAccessor = callContextAccessor;

            _messageBodyFactory = serializationProvider.CreateMessageBodyFactory();
            _messageBodySerializer = serializationProvider.CreateMessageBodySerializer();

            _requestChannel = Channel.CreateBounded<RequestData>(new BoundedChannelOptions(_options.DispatchConcurrency)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = true,
                AllowSynchronousContinuations = true
            });
        }

        private async Task ConnectLoopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RabbitServiceHost is starting...");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("Connecting to RabbitMQ server...");

                    _connection = _connectionProvider.CreateConnection();
                    _invokeChannel = _connection.CreateModel();
                    _cancellationChannel = _connection.CreateModel();

                    _invokeChannel.BasicQos(0, 16, false);
                    _cancellationChannel.BasicQos(0, 16, false);

                    _logger.LogDebug("Established channel with prefetch_count={PrefetchCount} for each consumer.", 8);

                    _cancellationChannel.ExchangeDeclare(DefaultManagementExchangeName, "fanout");

                    _logger.LogDebug("Using exchange '{ExchangeName}' for management traffic.", DefaultManagementExchangeName);

                    foreach (var svc in _options.ServiceDescriptors)
                    {
                        BindQueue(_invokeChannel, _cancellationChannel, svc.Value);
                    }

                    _logger.LogInformation("RabbitServiceHost started (HostId: {HostId}).", _hostId);

                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Failed to start RabbitServiceHost: " + ex.Message);

                    _invokeChannel?.Dispose();
                    _cancellationChannel?.Dispose();
                    _connection?.Dispose();
                }

                await Task.Delay(10000, cancellationToken);
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ConnectLoopAsync(cancellationToken);
            _ = Task.Run(() => ReadChannelAsync(_stopSignal.Token));
        }

        private async Task OnInvokeRequestAsync(object sender, BasicDeliverEventArgs ea)
        {
            var bufferOwner = MemoryPool<byte>.Shared.Rent(ea.Body.Length);
            ea.Body.CopyTo(bufferOwner.Memory);

            var req = new RequestData(ea.RoutingKey, ea.DeliveryTag, ea.BasicProperties, bufferOwner);
            await _requestChannel.Writer.WriteAsync(req);
        }

        private void OnCancelRequest(object sender, BasicDeliverEventArgs ea)
        {
            switch (ea.BasicProperties.Type)
            {
                case "Cancellation":
                    if (_ongoingRequests.TryGetValue(ea.BasicProperties.CorrelationId, out var c))
                    {
                        c.Cancel();
                        _logger.LogInformation("Request [{RequestId}] cancelled by client.", ea.BasicProperties.CorrelationId);
                    }
                    break;
                default:
                    break;
            }
        }

        private Task OnCancelRequestAsync(object sender, BasicDeliverEventArgs ea)
        {
            OnCancelRequest(sender, ea);
            return Task.CompletedTask;
        }

        private void BindQueue(IModel invokeChannel, IModel cancellationChannel, ServiceDescriptor descriptor)
        {
            var invokeQueue = invokeChannel.QueueDeclare(queue: "RabbitRPC:" + descriptor.Name, durable: false, exclusive: false, autoDelete: true).QueueName;

            var cancellationQueue = invokeChannel.QueueDeclare(exclusive: true).QueueName;
            invokeChannel.QueueBind(cancellationQueue, DefaultManagementExchangeName, descriptor.Name);


            var consumer = new AsyncEventingBasicConsumer(invokeChannel);
            consumer.Received += OnInvokeRequestAsync;

            var cancellationConsumer = new AsyncEventingBasicConsumer(cancellationChannel);
            cancellationConsumer.Received += OnCancelRequestAsync;

            invokeChannel.BasicConsume(invokeQueue, false, consumer);
            cancellationChannel.BasicConsume(cancellationQueue, true, cancellationConsumer);

            _logger.LogDebug(
                "Created queue '{RpcQueueName}' for RPC requests and queue '{ManagementQueueName}' for management messages.", invokeQueue, cancellationQueue);
        }

        private async Task ReadChannelAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var item = await _requestChannel.Reader.ReadAsync(cancellationToken);
                _ = ExecuteRequestAsync(item.RoutingKey, item.DeliveryTag, item.BasicProperties, item.Body);
            }
        }

        private async Task ExecuteRequestAsync(string routingKey, ulong deliveryTag, IBasicProperties props, IMemoryOwner<byte> bufferOwner)
        {
            var stopWatch = Stopwatch.StartNew();
            var serviceName = routingKey.Substring(10); // RabbitRPC:XXXXXX
            var correlationId = props.CorrelationId;
            var replyTo = props.ReplyTo;
            var action = props.Type;
            var body = bufferOwner.Memory;
            var descriptor = _options.ServiceDescriptors[serviceName];

            using var _1 = _logger.BeginScope(new Dictionary<string, object>
            {
                ["HostId"] = _hostId,
                ["RequestId"] = correlationId,
                ["ServiceName"] = descriptor.Name,
                ["RequestSize"] = body.Length,
                ["ResponseQueue"] = replyTo,
                ["ActionName"] = action,
            });

            _logger.LogDebug($"Received request '{correlationId}' with a {body.Length}-bytes payload to execute action '{descriptor.Name}.{action}'.");

            var cts = new CancellationTokenSource();
            _ongoingRequests[correlationId] = cts;
            var execution = 0;

            try
            {
            START:
                var rpcContext = new CallContext
                {
                    ActionName = action,
                    ServiceName = descriptor.Name,
                    HostId = _hostId,
                    RequestId = correlationId,
                    ExecutionId = execution++,
                    RequestProperties = props,
                    ResponseProperties = _invokeChannel!.CreateBasicProperties(),
                    RequestAborted = cts.Token,
                    RequestBody = _messageBodySerializer.DeserializeRequest(body),
                    ResponseBody = _messageBodyFactory.CreateResponse(routingKey, action)
                };

                using (var requestServices = _serviceProvider.CreateScope())
                {
                    rpcContext.RequestServices = requestServices.ServiceProvider;

                    _callContextAccessor.CallContext = rpcContext;

                    if (descriptor.Actions.TryGetValue(action, out var actionDesc))
                    {
                        _logger.LogDebug($"Request matched with {{action = \"{action}\", service = \"{serviceName}\"}}. " +
                            $"Executing action with signature {actionDesc.MethodInfo} on {actionDesc.MethodInfo.DeclaringType.FullName}.");

                        var actionContext = new ActionContext
                        {
                            ActionDescriptor = actionDesc,
                            CallContext = rpcContext,
                            ServiceDescriptor = descriptor
                        };

                        stopWatch.Start();
                        var resultContext = await InvokeActionAsync(actionContext);

                        if (resultContext.CallContext.RequestAborted.IsCancellationRequested)
                        {
                            rpcContext.ResponseBody.IsCanceled = true;
                        }
                        else if (resultContext.Exception != null && !resultContext.ExceptionHandled)
                        {
                            switch (resultContext.UnhandledExceptionHandlingStrategy)
                            {
                                case ExceptionHandlingStrategy.ReturnDefault:
                                    if (actionDesc.MethodInfo.ReturnType.IsGenericType)
                                    {
                                        var actualType = actionDesc.MethodInfo.ReturnType.GetGenericParameterConstraints()[0];
                                        if (actualType.IsValueType)
                                        {
                                            rpcContext.ResponseBody.SetReturnValue(Activator.CreateInstance(actualType));
                                            break;
                                        }
                                    }
                                    rpcContext.ResponseBody.SetReturnValue(null);
                                    break;
                                case ExceptionHandlingStrategy.ReExecute:
                                    goto START;
                                case ExceptionHandlingStrategy.RejectRequest:
                                    _invokeChannel.BasicNack(deliveryTag, false, true);
                                    break;
                                case ExceptionHandlingStrategy.PropagateToClient:
                                default:
                                    rpcContext.ResponseBody.Exception = resultContext.Exception;
                                    break;
                            }
                        }
                        else
                        {
                            rpcContext.ResponseBody.SetReturnValue(resultContext.Result);
                        }
                    }
                    else
                    {
                        var msg = $"Unable to find matching action for request with {{action = \"{action}\", service = \"{serviceName}\"}}.";
                        _logger.LogError(msg);
                        rpcContext.ResponseBody.Exception = new RabbitRpcServerException(msg);
                    }
                }

                var responseData = _messageBodySerializer.Serialize(rpcContext.ResponseBody);

                using var _2 = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["ResponseSize"] = responseData.Length,
                });

                rpcContext.ResponseProperties.CorrelationId = correlationId;
                _invokeChannel.BasicPublish("", replyTo, rpcContext.ResponseProperties, responseData);

                // Send ACK here because we've done here.
                _invokeChannel.BasicAck(deliveryTag, false);

                stopWatch.Stop();

                var rs = rpcContext.ResponseBody.Exception != null ? "ERROR" : rpcContext.ResponseBody.IsCanceled ? "CANCELLED" : "SUCCESS";
                _logger.LogInformation(
                    $"Request to finished in {stopWatch.Elapsed.TotalMilliseconds:F4}ms with {execution} execution(s)." +
                    $" - Status = {rs}, BytesReceived = {body.Length}, BytesSent = {responseData.Length}, Action = {rpcContext.ServiceName}.{rpcContext.ActionName}");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An unexpected error occurred when process RPC request: " + ex.Message);

                // Need to send NACK without requeue here to prevent RabbitMQ from keep disptaching this request which may cause infinite loop.
                _invokeChannel!.BasicNack(deliveryTag, false, false);
            }
            finally
            {
                bufferOwner.Dispose();

                _callContextAccessor.CallContext = null;

                if (_ongoingRequests.TryRemove(correlationId, out var c))
                {
                    c.Dispose();
                }
            }
        }

        private async Task<IActionExecutedContext> InvokeActionAsync(ActionContext actionContext)
        {
            var serviceInstance = (IRabbitService)actionContext.CallContext.RequestServices.GetRequiredService(actionContext.ServiceDescriptor.ServiceType);

            var filters = actionContext.ActionDescriptor.Filters
               .Concat(actionContext.ServiceDescriptor.Filters)
               .Concat(_options.Filters)
               .OrderBy(x => x.Order)
               .ThenBy(x => x.Scope)
               .Select(x => x.Filter is IFilterFactory fac ? fac.CreateInstance(actionContext.CallContext.RequestServices) : x.Filter)
               .ToList();

            if (serviceInstance is RabbitService rs)
            {
                filters.Insert(0, rs);
            }

            var parameters = new Dictionary<string, object?>();
            var executingContext = new ActionExecutingContext(actionContext, filters, parameters, serviceInstance);
            var executedContext = new ActionExecutedContext(actionContext, filters, serviceInstance);

            // Build a recursive filter pipeline to invoke all filters and action method.

            IFilterChain chain = new ChainTermination(new ActionExecutionDelegate(ExecuteActionAsync));
            var reversedFilters = filters.AsEnumerable().Reverse().ToArray();

            foreach (var filter in reversedFilters.Where(x=>x is IAsyncActionFilter || x is IActionFilter))
            {
                if(filter is IAsyncActionFilter af)
                {
                    chain = new AsyncActionFilterChain(chain, af, executingContext, executedContext);
                }
                else
                {
                    chain = new ActionFilterChain(chain, (IActionFilter)filter, executingContext, executedContext);
                }
            }
            foreach (var filter in reversedFilters.OfType<IParameterBindingFilter>())
            {
                chain = new ParameterBindingFilterChain(chain, filter, actionContext, parameters);
            }
            foreach (var filter in reversedFilters.OfType<IServiceInitializationFilter>())
            {
                chain = new ServiceInitializationFilterChain(chain, filter, actionContext, serviceInstance);
            }

            var sw = Stopwatch.StartNew();
            await chain.ExecuteAsync();
            sw.Stop();

            _logger.LogDebug($"Executed action {actionContext.ServiceDescriptor.Name}.{actionContext.ActionDescriptor.Name} in {sw.Elapsed.TotalMilliseconds:F4}ms.");

            return executedContext;

            async Task<IActionExecutedContext> ExecuteActionAsync()
            {
                var stopwatch = new Stopwatch();

                if (!executedContext.Canceled)
                {
                    _logger.LogDebug($"Executing action method {actionContext.ServiceDescriptor.ServiceType.Name}.{actionContext.ActionDescriptor.MethodInfo.Name}.");

                    stopwatch.Restart();

                    object? result = null;
                    try
                    {
                        var returnType = actionContext.ActionDescriptor.MethodInfo.ReturnType;
                        result = actionContext.ActionDescriptor.MethodInfo.Invoke(serviceInstance, executingContext!.ActionArguments.Values.ToArray());

                        if (result is Task task)
                        {
                            await task;

                            if (result.GetType().IsConstructedGenericType)
                            {
                                result = result.GetType().GetProperty("Result").GetValue(result);
                            }
                            else
                            {
                                result = null;
                            }
                        }

                        executedContext.Result = result;
                    }
                    catch (TargetInvocationException tie)
                    {
                        executedContext!.Exception = tie.InnerException;
                    }
                    catch (Exception ex)
                    {
                        executedContext!.Exception = ex;
                    }

                    stopwatch.Stop();

                    _logger.LogDebug($"Executed action method {actionContext.ServiceDescriptor.ServiceType.Name}.{actionContext.ActionDescriptor.MethodInfo.Name} " +
                    $"in {stopwatch.Elapsed.TotalMilliseconds:F4}ms. - Exception: {executedContext!.Exception?.GetType()?.Name ?? "null"}, Result: {result}");
                }

                return executedContext;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _invokeChannel?.Dispose();
            _cancellationChannel?.Dispose();
            _connection?.Dispose();

            _stopSignal.Cancel();

            return Task.CompletedTask;
        }

        private readonly struct RequestData
        {
            public RequestData(string routingKey, ulong deliveryTag, IBasicProperties props, IMemoryOwner<byte> body)
                => (RoutingKey, DeliveryTag, BasicProperties, Body) = (routingKey, deliveryTag, props, body);

            public string RoutingKey { get; }
            public IBasicProperties BasicProperties { get; }

            public ulong DeliveryTag { get; }

            public IMemoryOwner<byte> Body { get; }
        }
    }
}
