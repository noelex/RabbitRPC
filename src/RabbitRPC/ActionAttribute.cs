using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC
{
    /// <summary>
    /// Configure a RabbitRPC service action. This attribute is optional. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ActionAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets value, indicating the name of the service action. When not specified, RabbitRPC will use the name of the method as action name.
        /// </summary>
        public string? Name { get; set; }
    }
}
