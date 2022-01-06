using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.WorkQueues
{
    public interface IWorkQueue
    {
        void Post<T>(T workItem);
    }
}
