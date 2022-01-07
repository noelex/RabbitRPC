using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RabbitRPC
{
    [Serializable]
    public class ConcurrencyException:Exception
    {
        public ConcurrencyException() : base("Detected conflicting concurrent update/delete operations on state context.") { }

        public ConcurrencyException(string message) : base(message) { }

        public ConcurrencyException(SerializationInfo si, StreamingContext context) : base(si, context) { }
    }
}
