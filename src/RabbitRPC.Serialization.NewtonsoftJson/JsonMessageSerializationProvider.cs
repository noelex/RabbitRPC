using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.Serialization.NewtonsoftJson
{
    public class JsonMessageSerializationProvider : IMessageSerializationProvider
    {
        public IMessageBodyFactory CreateMessageBodyFactory() => new JsonMessageFactory();

        public IMessageBodySerializer CreateMessageBodySerializer() => new JsonMessageBodySerializer();
    }
}
