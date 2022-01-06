using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC
{
    [Serializable]
    public class ConcurrencyException:Exception
    {
        public ConcurrencyException() : base("The state is modified by another participant.") { }

        public ConcurrencyException(string message) : base(message) { }
    }
}
