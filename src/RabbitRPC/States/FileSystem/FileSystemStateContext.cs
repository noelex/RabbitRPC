using RabbitRPC.Serialization;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.States.FileSystem
{
    internal class FileSystemStateContext : IStateContext, IDisposable
    {
        private readonly string _fileName;
        private readonly IMessageBodySerializer _serializer;

        private FileSystemStateTransaction? _tx;

        public FileSystemStateContext(string fileName, IMessageSerializationProvider serializationProvider)
        {
            _fileName = fileName;
            _serializer = serializationProvider.CreateMessageBodySerializer();
        }

        public async Task<State<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var (local, tx) = await EnsureTransactionAsync(cancellationToken);
            try
            {
                return tx.TryGetValue(key, out var result) ? new State<T>((T)result!) : new State<T>();
            }
            finally
            {
                if (local)
                {
                    tx.Dispose();
                }
            }
        }

        public async Task<State<object?>[]> GetAsync(string[] keys, CancellationToken cancellationToken = default)
        {
            var (local, tx) = await EnsureTransactionAsync(cancellationToken);
            try
            {

                var result = new State<object?>[keys.Length];
                for (var i = 0; i < keys.Length; i++)
                {
                    result[i] = tx.TryGetValue(keys[i], out var value) ? new State<object?>(value!) : new State<object?>();
                }

                return result;
            }
            finally
            {
                if (local)
                {
                    tx.Dispose();
                }
            }

        }

        public async Task<State<T>[]> GetAsync<T>(string[] keys, CancellationToken cancellationToken = default)
        {
            var (local, tx) = await EnsureTransactionAsync(cancellationToken);
            try
            {
                var result = new State<T>[keys.Length];
                for (var i = 0; i < keys.Length; i++)
                {
                    result[i] = tx.TryGetValue(keys[i], out var value) ? new State<T>((T)value!) : new State<T>();
                }

                return result;
            }
            finally
            {
                if (local)
                {
                    tx.Dispose();
                }
            }
        }

        public void Dispose()
        {
            _tx?.Dispose();
        }

        public async Task<ITransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Default, CancellationToken cancellationToken = default)
        {
            if (_tx != null)
            {
                throw new InvalidOperationException("There is aleady an active transaction on current context.");
            }

            return _tx = await FileSystemStateTransaction.CreateAsync(isolationLevel, _fileName, _serializer, cancellationToken);
        }

        public Task<ITransaction> BeginTransactionAsync(CancellationToken token) => BeginTransactionAsync(IsolationLevel.Default, token);

        private async Task<(bool isLocal, FileSystemStateTransaction transaction)> EnsureTransactionAsync(CancellationToken cancellationToken)
            => _tx != null ? (false, _tx) : (true, await FileSystemStateTransaction.CreateAsync(IsolationLevel.Default, _fileName, _serializer, cancellationToken));


        public async Task PutAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        {
            var (local, tx) = await EnsureTransactionAsync(cancellationToken);
            try
            {
                tx.Put(key, value);
            }
            finally
            {
                if (local) tx.Dispose();
            }
        }

        public Task PutAsync<T>(string key, T value, long version, CancellationToken cancellationToken = default)
        {
            return PutAsync(key, value, cancellationToken);
        }

        public async Task PutAsync(IDictionary<string, object?> keyValuePairs, CancellationToken cancellationToken = default)
        {
            var (local, tx) = await EnsureTransactionAsync(cancellationToken);
            try
            {
                foreach (var (k, v) in keyValuePairs)
                {
                    tx.Put(k, v);
                }
            }
            finally
            {
                if (local) tx.Dispose();
            }
        }

        public async Task PutAsync<T>(IDictionary<string, T> keyValuePairs, CancellationToken cancellationToken = default)
        {
            var (local, tx) = await EnsureTransactionAsync(cancellationToken);
            try
            {
                foreach (var (k, v) in keyValuePairs)
                {
                    tx.Put(k, v);
                }
            }
            finally
            {
                if (local) tx.Dispose();
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            var (local, tx) = await EnsureTransactionAsync(cancellationToken);
            try
            {
                tx.Remove(key);
            }
            finally
            {
                if (local) tx.Dispose();
            }
        }

        public Task RemoveAsync(string key, long version, CancellationToken cancellationToken)
        {
            return RemoveAsync(key, cancellationToken);
        }
    }
}
