using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using RabbitRPC.States;
using RabbitRPC.States.EntityFrameworkCore;
using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class EFStateContextServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkCoreStateContext(this IServiceCollection services, Action<DbContextOptionsBuilder> configure, StateTableColumnTypes? customColumnTypes=null)
            => services.AddEntityFrameworkCoreStateContext<SqlCommandExecutorFactory>(configure, customColumnTypes);

        public static IServiceCollection AddEntityFrameworkCoreStateContext<TCommandExecutorFactory>(this IServiceCollection services, Action<DbContextOptionsBuilder> configure, StateTableColumnTypes? customColumnTypes = null)
            where TCommandExecutorFactory : class, ISqlCommandExecutorFactory
        {
            var builder = new DbContextOptionsBuilder<StateDbContext>();
            configure(builder);

            return services
                .AddSingleton<ISqlCommandExecutorFactory, TCommandExecutorFactory>()
                .AddSingleton<IStateContextFactory>(
                    sp => new EFStateContextFactory(builder.Options, sp.GetRequiredService<IMessageSerializationProvider>(), sp.GetRequiredService<ISqlCommandExecutorFactory>(), customColumnTypes));
        }
    }
}
