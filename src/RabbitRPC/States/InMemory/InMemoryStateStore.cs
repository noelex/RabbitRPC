using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.States.InMemory
{
    internal class InMemoryStateStore:IDisposable, IStateStore
    {
        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly Dictionary<string, object?> _store=new Dictionary<string, object?>();
        private ActionDisposable? _lock;

        public void Put(string key, object? value)
        {
            _store[key] = value;
        }

        public bool TryGetValue(string key, out object? value) => _store.TryGetValue(key, out value);

        public bool Remove(string key) => _store.Remove(key);

        public IDictionary<string, object?> Store => _store;

        public async Task<ActionDisposable> LockAsync(CancellationToken cancellationToken)
        {
            _lock = await _asyncLock.LockAsync(cancellationToken);
            return _lock;
        }

        public void Dispose()
        {
            _lock?.Dispose();
            _asyncLock.Dispose();
            _store.Clear();
        }
    }
}
