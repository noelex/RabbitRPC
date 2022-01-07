using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.States.EntityFrameworkCore.Executors
{
    internal class SqliteCommandExecutor : ISqlCommandExecutor
    {
        public Task<int> AddOrUpdateAsync(DbContext database, string key, byte[] value, long version, CancellationToken cancellationToken)
        {
            return database.Database
               .ExecuteSqlInterpolatedAsync(
                   $"INSERT INTO StateEntry(Key,Value,Version) VALUES({key},{value},0) ON CONFLICT(Key) DO UPDATE SET Value={value}, Version=Version+1 WHERE Key={key} AND Version={version};", cancellationToken);
        }

        public Task<int> AddOrUpdateAsync(DbContext database, string key, byte[] value, CancellationToken cancellationToken)
        {
            return database.Database
               .ExecuteSqlInterpolatedAsync(
                   $"INSERT INTO StateEntry(Key,Value,Version) VALUES({key},{value},0) ON CONFLICT(Key) DO UPDATE SET Value={value}, Version=Version+1 WHERE Key={key};", cancellationToken);
        }

        public Task<int> DeleteAsync(DbContext database, string key, long version, CancellationToken cancellationToken)
        {
            return database.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM StateEntry WHERE Key={key} AND Version={version}", cancellationToken);
        }

        public Task<int> DeleteAsync(DbContext database, string key, CancellationToken cancellationToken)
        {
            return database.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM StateEntry WHERE Key={key}", cancellationToken);
        }
    }
}
