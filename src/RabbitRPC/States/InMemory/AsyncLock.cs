using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.States.InMemory
{
    internal sealed class AsyncLock : IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public void Dispose()
        {
            _semaphore.Dispose();
        }

        public async Task<ActionDisposable> LockAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);
            return new ActionDisposable(() => _semaphore.Release());
        }
    }
}
