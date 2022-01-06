using System.Collections.Generic;

namespace RabbitRPC.States.FileSystem
{
    internal interface IPendingOperation
    {
        void Execute(IStateStore store);

        void Execute(IDictionary<string, object?> keyValuePairs);
    }

    struct PendingRemove : IPendingOperation
    {
        private readonly string _key;

        public PendingRemove(string key) => _key = key;

        public void Execute(IStateStore store)
        {
            store.Remove(_key);
        }

        public void Execute(IDictionary<string, object?> keyValuePairs)
        {
            keyValuePairs.Remove(_key);
        }
    }

    struct PendingPut : IPendingOperation
    {
        private readonly string _key;
        private readonly object? _value;

        public PendingPut(string key, object? value) => (_key, _value) = (key, value);

        public void Execute(IStateStore store)
        {
            store.Put(_key, _value);
        }

        public void Execute(IDictionary<string, object?> keyValuePairs)
        {
            keyValuePairs[_key] = _value;
        }
    }
}
