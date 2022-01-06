using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Serialization.MessagePack
{
    public class MessagePackMessageSerializationProvider : IMessageSerializationProvider
    {
        public IMessageBodyFactory CreateMessageBodyFactory() => new MessagePackMessageFactory();

        public IMessageBodySerializer CreateMessageBodySerializer() => new MessagePackMessageBodySerializer();
    }
}
