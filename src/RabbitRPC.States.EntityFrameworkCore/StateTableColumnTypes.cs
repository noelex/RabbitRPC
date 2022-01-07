using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.States.EntityFrameworkCore
{
    public class StateTableColumnTypes
    {
        public StateTableColumnTypes(string keyColumnType, string valueColumnType, string versionColumnType)
            => (KeyColumnType, ValueColumnType, VersionColumnType) = (keyColumnType, valueColumnType, versionColumnType);

        public string KeyColumnType { get; }

        public string ValueColumnType { get; }

        public string VersionColumnType { get; }
    }
}
