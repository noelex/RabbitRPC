using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC
{
    /// <summary>
    /// Configure a RabbitRPC service. This attribute is optional. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class RabbitServiceAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets value, indicating the name of the service. When not specified, RabbitRPC will use the name of the type as service name.
        /// </summary>
        public string? Name { get; set; }
    }
}
