using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using RabbitRPC.Serialization;
using RabbitRPC.States.EntityFrameworkCore;
using RabbitRPC.States.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.States.EntityFrameworkCore
{
    internal class EFStateContextFactory : IStateContextFactory
    {
        private readonly IMessageBodySerializer _serializer;
        private readonly DbContextOptions<StateDbContext> _options;
        private readonly ISqlCommandExecutorFactory _executorFactory;
        private readonly StateTableColumnTypes? _customColumnTypes;

        private bool _migrated = false;

        public EFStateContextFactory(DbContextOptions<StateDbContext> options , 
            IMessageSerializationProvider serializationProvider, ISqlCommandExecutorFactory executorFactory, StateTableColumnTypes? customColumnTypes)
        {
            _options = options;
            _executorFactory = executorFactory;
            _customColumnTypes = customColumnTypes;
            _serializer = serializationProvider.CreateMessageBodySerializer();
        }

        public IStateContext CreateStateContext(string name)
        {
            var db = new StateDbContext(_options);
            if (!_migrated)
            {
                CreateDb.CustomColumnTypes = _customColumnTypes;

                db.Database.Migrate();
                _migrated = true;
            }

            return new EFStateContext(name, db, _serializer, _executorFactory.CreateSqlCommandExecutor(db));
        }
    }
}
