using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RabbitRPC.States.Sqlite.Models
{
    class StateEntry
    {
        public string Key { get; set; } = null!;

        public byte[] Value { get; set; } = null!;

        public long Version { get; set; }
    }
}
