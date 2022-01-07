using RabbitRPC.ServiceHost.Filters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace RabbitRPC.ServiceHost
{
    public class ServiceDescriptor
    {
        internal ServiceDescriptor(InterfaceMapping serviceMethodMapping)
        {
            Name = serviceMethodMapping.InterfaceType.FullName;
            ServiceType = serviceMethodMapping.TargetType;

            var actions = new Dictionary<string, ActionDescriptor>();
            for(var i = 0; i < serviceMethodMapping.InterfaceMethods.Length; i++)
            {
                var im = serviceMethodMapping.InterfaceMethods[i];
                var tm = serviceMethodMapping.TargetMethods[i];

                if (!im.IsSpecialName)
                {
                    var desc = new ActionDescriptor(tm, im);
                    actions.Add(desc.Name, desc);
                }
                else
                {
                    throw new NotSupportedException("Declaring properties and events on RabbitRPC service interfaces are not supported.");
                }
            }

            Actions = new ReadOnlyDictionary<string, ActionDescriptor>(actions);
            Name = serviceMethodMapping.InterfaceType.GetCustomAttribute<RabbitServiceAttribute>()?.Name ?? serviceMethodMapping.InterfaceType.FullName;
            AddFilters(serviceMethodMapping.TargetType.GetCustomAttributes().OfType<IFilterMetadata>());
        }

        private void AddFilters(IEnumerable<IFilterMetadata> filters)
        {
            foreach (var f in filters)
            {
                Filters.Add(new FilterDescriptor(f, f is IOrderedFilter o ? o.Order : 0, FilterScope.Service));
            }
        }

        public Type ServiceType { get; }

        public IReadOnlyDictionary<string, ActionDescriptor> Actions { get; }

        public IList<FilterDescriptor> Filters { get; }=new List<FilterDescriptor>();

        public string Name { get; }
    }
}
