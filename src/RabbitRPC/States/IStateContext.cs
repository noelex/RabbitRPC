using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.States
{
    public interface IStateContext
    {
        Task PutAsync<T>(string key, T value, CancellationToken cancellationToken = default);

        Task PutAsync<T>(string key, T value, long version, CancellationToken cancellationToken = default);

        Task PutAsync(IDictionary<string, object?> keyValuePairs, CancellationToken cancellationToken = default);

        Task PutAsync<T>(IDictionary<string, T> keyValuePairs, CancellationToken cancellationToken = default);

        Task RemoveAsync(string key, CancellationToken cancellationToken);

        Task RemoveAsync(string key, long version, CancellationToken cancellationToken);

        Task<State<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        Task<State<object?>[]> GetAsync(string[] keys, CancellationToken cancellationToken=default);

        Task<State<T>[]> GetAsync<T>(string[] keys, CancellationToken cancellationToken = default);

        Task<ITransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken= default);

        Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
