using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.States.EntityFrameworkCore.Models
{
    public class StateEntry
    {
        public string Key { get; set; } = null!;

        public byte[] Value { get; set; } = null!;

        public long Version { get; set; }
    }
}
