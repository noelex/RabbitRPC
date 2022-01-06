using RabbitRPC.ServiceHost.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitRPC.ServiceHost
{
    public class ActionDescriptor
    {
        public ActionDescriptor(MethodInfo methodInfo)
        {
            if(typeof(Task).IsAssignableFrom( methodInfo.ReturnType) ||
                methodInfo.ReturnType==typeof(ValueTask) ||
                methodInfo.ReturnType.GetGenericTypeDefinition()== typeof(ValueTask<>))
            {
                MethodInfo = methodInfo;
            }
            else
            {
                throw new NotSupportedException("Action method must return a Task/ValueTask or their generic variants.");
            }

            AddFilters(methodInfo.GetCustomAttributes().OfType<IFilterMetadata>());

            Parameters = methodInfo.GetParameters().Select(x=>new ParameterDescriptor(x)).ToList();
        }

        private void AddFilters(IEnumerable<IFilterMetadata> filters)
        {
            foreach (var f in filters)
            {
                Filters.Add(new FilterDescriptor(f, f is IOrderedFilter o ? o.Order : 0, FilterScope.Action));
            }
        }

        public MethodInfo MethodInfo { get; }

        public IList<ParameterDescriptor> Parameters { get; }

        public IList<FilterDescriptor> Filters { get; } = new List<FilterDescriptor>();

        public string Name => MethodInfo.Name;
    }
}
