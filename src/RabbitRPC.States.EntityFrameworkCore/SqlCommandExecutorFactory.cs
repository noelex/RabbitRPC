using Microsoft.EntityFrameworkCore;
using RabbitRPC.States.EntityFrameworkCore.Executors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.States.EntityFrameworkCore
{
    public interface ISqlCommandExecutorFactory
    {
        ISqlCommandExecutor CreateSqlCommandExecutor(DbContext dbContext);
    }

    internal class SqlCommandExecutorFactory : ISqlCommandExecutorFactory
    {
        private ConcurrentDictionary<string, ISqlCommandExecutor> _executors = new ConcurrentDictionary<string, ISqlCommandExecutor>();

        public ISqlCommandExecutor CreateSqlCommandExecutor(DbContext dbContext)
        {
            return _executors.GetOrAdd(dbContext.Database.ProviderName, name => name switch
            {
                "Microsoft.EntityFrameworkCore.Sqlite" => new SqliteCommandExecutor(),
                "Microsoft.EntityFrameworkCore.SqlServer" => new SqlServerCommandExecutor(),
                "Microsoft.EntityFrameworkCore.PostgreSQL" => new SqliteCommandExecutor(),
                _ => throw new NotSupportedException($"Cannot create SqlCommandExecutor for '{dbContext.Database.ProviderName}' as it's not supported.")
            });
        }
    }
}
