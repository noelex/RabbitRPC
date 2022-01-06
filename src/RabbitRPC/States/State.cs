using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.States
{
    public struct State<T>
    {
        public State(T value, long version=0) => (HasValue, Value, Version) = (true, value, version);

        public T Value { get; }

        public bool HasValue { get; }

        public long Version { get; }
    }
}
