using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.States.EntityFrameworkCore.Executors
{
    internal class SqlServerCommandExecutor : ISqlCommandExecutor
    {
        public Task<int> AddOrUpdateAsync(DbContext database, string key, byte[] value, long version, CancellationToken cancellationToken)
        {
            return database.Database
               .ExecuteSqlInterpolatedAsync(
                   $@"MERGE INTO StateEntry WITH (HOLDLOCK) AS o
                      USING (SELECT {key} AS [Key],{value} AS Value, {version} AS Version) AS n
                      ON (o.[Key] = n.[Key])
                      WHEN MATCHED AND o.Version=n.Version THEN UPDATE SET o.Value=n.Value, o.Version=o.Version+1
                      WHEN NOT MATCHED THEN INSERT ([Key],Value,Version) VALUES (n.[Key],n.Value,0);", cancellationToken);
        }

        public Task<int> AddOrUpdateAsync(DbContext database, string key, byte[] value, CancellationToken cancellationToken)
        {
            return database.Database
               .ExecuteSqlInterpolatedAsync(
                   $@"MERGE INTO StateEntry WITH (HOLDLOCK) AS o
                      USING (SELECT {key} AS [Key],{value} AS Value) AS n
                      ON (o.[Key] = n.[Key])
                      WHEN MATCHED THEN UPDATE SET o.Value=n.Value, o.Version=o.Version+1
                      WHEN NOT MATCHED THEN INSERT ([Key],Value,Version) VALUES (n.[Key],n.Value,0);", cancellationToken);
        }

        public Task<int> DeleteAsync(DbContext database, string key, long version, CancellationToken cancellationToken)
        {
            return database.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM StateEntry WHERE [Key]={key} AND Version={version}", cancellationToken);
        }

        public Task<int> DeleteAsync(DbContext database, string key, CancellationToken cancellationToken)
        {
            return database.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM StateEntry WHERE [Key]={key}", cancellationToken);
        }
    }
}
