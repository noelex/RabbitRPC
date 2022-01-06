using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RabbitRPC.ServiceHost
{
    public class ParameterDescriptor
    {
        public ParameterDescriptor(ParameterInfo pi)
        {
            (Name, ParameterType) = (pi.Name, pi.ParameterType);

            if (pi.IsOut)
            {
                throw new NotSupportedException("Out/Ref parameters are not supported. Parameter name: " + pi.Name);
            }
        }

        public string Name { get; }

        public Type ParameterType { get; }
    }
}
