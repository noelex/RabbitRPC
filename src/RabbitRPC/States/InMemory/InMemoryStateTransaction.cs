using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.States.InMemory
{
    internal class InMemoryStateTransaction : ITransaction, IStateStore
    {
        private IDictionary<string, object?>? _snapshot;
        private readonly IDictionary<string, object?> _store;
        private readonly ActionDisposable _rwLock;

        public InMemoryStateTransaction(IDictionary<string, object?> store, ActionDisposable rwLock)
        {
            _store = store;
           _rwLock= rwLock;
        }

        private void EnsureSnapshot()
        {
            if (_snapshot == null)
            {
                _snapshot = new Dictionary<string, object?>(_store);
            }
        }

        public bool TryGetValue(string key, out object? value)
        {
            return (_snapshot??_store).TryGetValue(key, out value);
        }

        public void Put(string key, object? value)
        {
            EnsureSnapshot();
            _snapshot![key] = value;
        }

        public bool Remove(string key)
        {
            EnsureSnapshot();
            return _snapshot!.Remove(key);
        }

        public Task CommitAsync(CancellationToken cancellationToken)
        {
            _store.Clear();
            foreach (var (k, v) in _snapshot!)
            {
                _store[k] = v;
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _rwLock.Dispose();
        }

        public Task RollbackAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
