using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.States
{
    internal interface IStateStore
    {

        bool TryGetValue(string key, out object? value);

        void Put(string key, object? value);

        bool Remove(string key);
    }
}
