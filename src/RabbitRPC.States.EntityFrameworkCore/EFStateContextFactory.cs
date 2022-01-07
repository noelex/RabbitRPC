using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitRPC.Serialization;
using RabbitRPC.States.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.States.Sqlite
{
    internal class EFStateContextFactory : IStateContextFactory
    {
        private readonly IMessageBodySerializer _serializer;
        private readonly DbContextOptions<StateDbContext> _options;
        private readonly ISqlCommandExecutorFactory _executorFactory;

        private bool _migrated = false;

        public EFStateContextFactory(DbContextOptions<StateDbContext> options , IMessageSerializationProvider serializationProvider, ISqlCommandExecutorFactory executorFactory)
        {
            _options = options;
            _executorFactory = executorFactory;
            _serializer = serializationProvider.CreateMessageBodySerializer();
        }

        public IStateContext CreateStateContext(string name)
        {
            var db = new StateDbContext(_options);
            if (!_migrated)
            {
                db.Database.Migrate();
                _migrated = true;
            }

            return new EFStateContext(name, db, _serializer, _executorFactory.CreateSqlCommandExecutor(db));
        }
    }
}
