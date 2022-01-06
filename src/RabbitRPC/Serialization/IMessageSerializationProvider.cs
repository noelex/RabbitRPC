using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Serialization
{
    public interface IMessageSerializationProvider
    {
        IMessageBodyFactory CreateMessageBodyFactory();

        IMessageBodySerializer CreateMessageBodySerializer();
    }
}
