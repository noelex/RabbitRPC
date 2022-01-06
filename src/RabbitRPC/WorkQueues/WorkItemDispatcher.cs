using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RabbitRPC.WorkQueues
{
    interface IWorkItemDispatcher: IDisposable
    {
        Task RunAsync(CancellationToken cancellationToken=default);
    }

    internal class WorkItemDispatcher<T> : IWorkItemDispatcher, IObserver<WorkItem<T>>
    {
        private readonly IDisposable _subscription;
        private readonly HandlerSpec _handlerSpec;
        private readonly IServiceProvider _serviceProvider;

        private readonly Channel<WorkItem<T>> _itemQueue;
        private readonly ILogger _logger;

        private readonly CancellationTokenSource _stopSignal=new CancellationTokenSource();
        private bool _disposed;

        public WorkItemDispatcher(IServiceProvider serviceProvider, IRabbitEventBus eventBus, HandlerSpec handlerSpec)
        {
            var fac = serviceProvider.GetRequiredService<ILoggerFactory>();
            _logger = fac.CreateLogger(string.Format(LoggerCategory.WorkItemHandler, typeof(T).Name));

            _handlerSpec = handlerSpec;
            _serviceProvider = serviceProvider;
            _itemQueue = Channel.CreateBounded<WorkItem<T>>(new BoundedChannelOptions(handlerSpec.Options.BufferSize)
            {
                AllowSynchronousContinuations = true,
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = true
            });

            _subscription = eventBus.Observe(handlerSpec.WorkItemType.FullName, 
                (ex, k, s) => new WorkItemSubject<T>(ex, k, s, "RabbitRPC.WorkQueue:"+ handlerSpec.WorkItemType.FullName))
                .Subscribe(this);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _subscription.Dispose();

                _stopSignal.Cancel();
                _stopSignal.Dispose();
                _disposed = true;
            }
        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnNext(WorkItem<T> value)
        {
            if (!_itemQueue.Writer.TryWrite(value))
            {
                value.Channel.BasicNack(value.Data.DeliveryTag, false, true);
            }
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_stopSignal.Token, cancellationToken);
            cancellationToken = cts.Token;

            var tasks = new List<Task<int>>();

            var batchTimeout = _handlerSpec.Options.BatchTimeout;
            var batchSize = _handlerSpec.Options.BatchSize;
            var degreeOfParallelism = _handlerSpec.Options.DegreeOfParallelism;

            using var bufferOwner = MemoryPool<WorkItem<T>>.Shared.Rent(batchSize * degreeOfParallelism);
            var items = bufferOwner.Memory;

            var waitUntilTimeout = batchTimeout > 0;

            _logger.LogInformation($"Started dispatcher for work items of type {typeof(T).FullName}." +
                $" - DegreeOfParallelism = {degreeOfParallelism}, BatchSize = {batchSize}," +
                $" BatchTimeout = {batchTimeout}ms, ConcurrencyMode = {_handlerSpec.Options.ConcurrencyMode}");
            var stopwatch = new Stopwatch();

            while (!cancellationToken.IsCancellationRequested)
            {
                tasks.Clear();
                bufferOwner.Memory.Span.Fill(null!);

                await _itemQueue.Reader.WaitToReadAsync(cancellationToken);

                stopwatch.Restart();

                using var timeoutCts = waitUntilTimeout ? new CancellationTokenSource(batchTimeout) : new CancellationTokenSource();
                using var mergedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                var count = 0;
                try
                {
                    while (count < batchSize * degreeOfParallelism)
                    {
                        if (_itemQueue.Reader.TryRead(out var item))
                        {
                            items.Span[count++] = item;
                        }
                        else
                        {
                            if (waitUntilTimeout)
                            {
                                await _itemQueue.Reader.WaitToReadAsync(mergedCts.Token);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
                {
                    // Reaching here means that BatchTimeout has reached. We should proceed anyway.
                }

                if (count <= 0)
                {
                    // We got nothing to process.
                    break;
                }

                stopwatch.Stop();
                _logger.LogDebug($"{count} work items read from queue in {stopwatch.Elapsed.TotalMilliseconds}ms.");

                stopwatch.Restart();

                var availableItems = items[..count];
                var batchCount = count / batchSize + (count % batchSize == 0 ? 0 : 1);
                var successCount = 0;
                using var scope = _serviceProvider.CreateScope();

                try
                {
                    for (var i = 0; i < batchCount; i++)
                    {
                        var lastBatch = i == batchCount - 1;
                        var batch = lastBatch ? availableItems.Slice(i * batchSize) : availableItems.Slice(i * batchSize, batchSize);
                        tasks.Add(ProcessBatchAsync(i, batchCount, scope.ServiceProvider, batch, cancellationToken));
                    }

                    successCount = (await Task.WhenAll(tasks)).Sum();
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, $"An unexpected error occurred when processing a parallel batch with {count} items: " + ex.Message);
                }

                stopwatch.Stop();
                _logger.LogDebug(
                    $"Finished processing {count} items with {batchCount} batches in {stopwatch.Elapsed.TotalMilliseconds}ms. {successCount}/{count} items are marked as done.");
            }
        }

        private async Task<int> ProcessBatchAsync(int id, int total, IServiceProvider serviceProvider, Memory<WorkItem<T>> workItems, CancellationToken cancellationToken)
        {
            IServiceScope? ownScope = null;
            if (_handlerSpec.Options.ConcurrencyMode == BatchConcurrencyMode.Isolated)
            {
                ownScope = serviceProvider.CreateScope();
                serviceProvider = ownScope.ServiceProvider;
            }

            try
            {
                var handler = serviceProvider.GetRequiredService(_handlerSpec.HandlerType);
                await (Task)_handlerSpec.HandlerType.GetMethod("ProcessAsync").Invoke(handler, new object[] { (ReadOnlyMemory<WorkItem<T>>)workItems, cancellationToken });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    $"An error occurred when processing batch {id}/{total} with handler {_handlerSpec.HandlerType.Name}: " + ex.Message);
            }
            finally
            {
                ownScope?.Dispose();
            }

            return SendAck(workItems, _logger);

            static int SendAck(Memory<WorkItem<T>> items, ILogger logger)
            {
                var i = 0;
                foreach (ref var item in items.Span)
                {
                    try
                    {
                        if (item.IsDone)
                        {
                            item.Channel.BasicAck(item.Data.DeliveryTag, false);
                            i++;
                        }
                        else
                        {
                            item.Channel.BasicNack(item.Data.DeliveryTag, false, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogCritical(ex,
                            $"Failed to send ACK/NACK for work item {typeof(T).Name}. - DeliveryTag = {item.Data.DeliveryTag}, IsDone = {item.IsDone}, Error = {ex.Message}");
                    }
                }

                return i;
            }
        }
    }
}
