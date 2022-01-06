using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using RabbitRPC.States;
using RabbitRPC.States.Sqlite;
using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SqliteStateContextServiceCollectionExtensions
    {
        public static IServiceCollection AddSqliteStateContext(this IServiceCollection services, string connectionString, Action<SqliteDbContextOptionsBuilder>? configure = null)
        {
            var builder = new DbContextOptionsBuilder<StateDbContext>();
            builder.UseSqlite(connectionString, configure);

            return services.AddSingleton<IStateContextFactory>(
                 sp => new SqliteStateContextFactory(builder.Options, sp.GetRequiredService<IMessageSerializationProvider>()));
        }
    }
}
