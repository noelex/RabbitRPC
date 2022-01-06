using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.States.InMemory
{
    internal class InMemoryStateContextFactory : IStateContextFactory, IDisposable
    {
        private ConcurrentDictionary<string, InMemoryStateStore> _contexts = new ConcurrentDictionary<string, InMemoryStateStore>();

        public IStateContext CreateStateContext(string name)
        {
            var store = _contexts.GetOrAdd(name, new InMemoryStateStore());
            return new InMemoryStateContext(store);
        }

        public void Dispose()
        {
            foreach(var v in _contexts.Values)
            {
                v.Dispose();
            }
        }
    }
}
