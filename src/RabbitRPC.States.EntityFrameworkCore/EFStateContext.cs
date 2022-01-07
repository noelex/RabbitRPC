using Microsoft.EntityFrameworkCore;
using RabbitRPC.Serialization;
using RabbitRPC.States.EntityFrameworkCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.States.EntityFrameworkCore
{
    internal class EFStateContext : IStateContext, IDisposable
    {
        private readonly StateDbContext _stateDbContext;
        private readonly string _namespace;
        private readonly IMessageBodySerializer _serializer;
        private readonly ISqlCommandExecutor _executor;

        public EFStateContext(string @namespace, StateDbContext stateDbContext, IMessageBodySerializer msp, ISqlCommandExecutor executor)
        {
            _stateDbContext = stateDbContext;
            _serializer = msp;
            _namespace = @namespace;
            _executor = executor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string PrefixWithNamespace(string key) => string.Format("{0}.{1}", _namespace, key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private State<T> ToState<T>(StateEntry? stateEntry)
             => stateEntry == null ? new State<T>() : new State<T>((T)_serializer.DeserializeWithTypeInfo(stateEntry.Value)!, stateEntry.Version);

        private static System.Data.IsolationLevel MapIsolationLevel(IsolationLevel isolationLevel)
            => isolationLevel switch
            {
                IsolationLevel.Chaos => System.Data.IsolationLevel.Chaos,
                IsolationLevel.ReadCommitted => System.Data.IsolationLevel.ReadCommitted,
                IsolationLevel.ReadUncommited => System.Data.IsolationLevel.ReadUncommitted,
                IsolationLevel.RepeatableRead => System.Data.IsolationLevel.RepeatableRead,
                IsolationLevel.Serializable => System.Data.IsolationLevel.Serializable,
                IsolationLevel.Snapshot => System.Data.IsolationLevel.Snapshot,
                _ => throw new NotSupportedException()

            };

        public async Task<ITransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
        {
            if (isolationLevel == IsolationLevel.Default)
            {
                return await BeginTransactionAsync(cancellationToken);
            }

            return new EFStateTransaction(await _stateDbContext.Database.BeginTransactionAsync(MapIsolationLevel(isolationLevel), cancellationToken));
        }

        public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return new EFStateTransaction(await _stateDbContext.Database.BeginTransactionAsync(cancellationToken));
        }

        public async Task<State<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            key = PrefixWithNamespace(key);
            var entry = await _stateDbContext.Set<StateEntry>().FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
            return ToState<T>(entry);
        }

        public async Task<State<object?>[]> GetAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            keys = keys.Select(x => PrefixWithNamespace(x)).ToArray();
            var items = await _stateDbContext.Set<StateEntry>().Where(x => keys.Contains(x.Key)).ToArrayAsync(cancellationToken);
            return items.Select(x => ToState<object?>(x)).ToArray();
        }

        public async Task<State<T>[]> GetAsync<T>(string[] keys, CancellationToken cancellationToken = default)
        {
            keys = keys.Select(x => PrefixWithNamespace(x)).ToArray();
            var items = await _stateDbContext.Set<StateEntry>().Where(x => keys.Contains(x.Key)).ToArrayAsync(cancellationToken);
            return items.Select(x => ToState<T>(x)).ToArray();
        }

        public Task PutAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        {
            key = PrefixWithNamespace(key);
            var mem = _serializer.SerializeWithTypeInfo(value);
            var buffer = mem.ToArray();

            return _executor.AddOrUpdateAsync(_stateDbContext, key, buffer, cancellationToken);
        }

        public async Task PutAsync<T>(string key, T value, long version, CancellationToken cancellationToken = default)
        {
            key = PrefixWithNamespace(key);
            var mem = _serializer.SerializeWithTypeInfo(value);
            var buffer = mem.ToArray();

            var count = await _executor.AddOrUpdateAsync(_stateDbContext, key, buffer, version, cancellationToken);

            if (count != 1)
            {
                throw new ConcurrencyException();
            }
        }

        public async Task PutAsync(IDictionary<string, object?> keyValuePairs, CancellationToken cancellationToken = default)
        {
            var tx = _stateDbContext.Database.CurrentTransaction == null
                ? await _stateDbContext.Database.BeginTransactionAsync(cancellationToken) : null;
            try
            {
                foreach (var (k, v) in keyValuePairs)
                {
                    await PutAsync(k, v, cancellationToken);
                }

                if (tx != null)
                {
                    await tx.CommitAsync(cancellationToken);
                }
            }
            finally
            {
                if (tx != null)
                {
                    await tx.DisposeAsync();
                }
            }
        }

        public async Task PutAsync<T>(IDictionary<string, T> keyValuePairs, CancellationToken cancellationToken = default)
        {
            var tx = _stateDbContext.Database.CurrentTransaction == null
                ? await _stateDbContext.Database.BeginTransactionAsync(cancellationToken) : null;

            try
            {
                foreach (var (k, v) in keyValuePairs)
                {
                    await PutAsync(k, v, cancellationToken);
                }

                if (tx != null)
                {
                    await tx.CommitAsync(cancellationToken);
                }
            }
            finally
            {
                if (tx != null)
                {
                    await tx.DisposeAsync();
                }
            }
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            key = PrefixWithNamespace(key);
            return _executor.DeleteAsync(_stateDbContext, key, cancellationToken);
        }

        public async Task RemoveAsync(string key, long version, CancellationToken cancellationToken)
        {
            key = PrefixWithNamespace(key);
            var count = await _executor.DeleteAsync(_stateDbContext, key, version, cancellationToken);
            if (count != 1)
            {
                throw new ConcurrencyException();
            }
        }

        public void Dispose()
        {
            _stateDbContext.Dispose();
        }
    }
}
