using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.States.Sqlite
{
    internal class SqliteStateContextFactory : IStateContextFactory
    {
        private readonly IMessageBodySerializer _serializer;
        private readonly DbContextOptions<StateDbContext> _options;

        private bool _migrated = false;

        public SqliteStateContextFactory(DbContextOptions<StateDbContext> options , IMessageSerializationProvider serializationProvider)
        {
            _options = options;
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
            
            return new SqliteStateContext(name, db, _serializer);
        }
    }
}
