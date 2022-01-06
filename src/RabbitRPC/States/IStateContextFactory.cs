using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.States
{
    public interface IStateContextFactory
    {
        IStateContext CreateStateContext(string name);
    }
}
