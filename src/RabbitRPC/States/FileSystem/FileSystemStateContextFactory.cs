using Microsoft.Extensions.DependencyInjection;
using RabbitRPC.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.States.FileSystem
{
    internal class FileSystemStateContextFactory : IStateContextFactory
    {
        private readonly string _dir;
        private readonly IServiceProvider _serviceProvider;

        public FileSystemStateContextFactory(string stateDir, IServiceProvider serviceProvider)
        {
            _dir = stateDir;
            _serviceProvider = serviceProvider;
        }

        public IStateContext CreateStateContext(string name)
        {
            return new FileSystemStateContext(Path.Combine(_dir, name + ".state"), _serviceProvider.GetRequiredService<IMessageSerializationProvider>());
        }
    }
}
