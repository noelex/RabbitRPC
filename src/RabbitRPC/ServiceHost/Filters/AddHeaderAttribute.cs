using System;

namespace RabbitRPC.ServiceHost.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AddHeaderAttribute : ActionFilterAttribute
    {
        public AddHeaderAttribute(string key, string value) => (Key, Value) = (key, value);

        public string Key { get; set; }

        public string Value { get; set; }

        public override void OnActionExecuted(IActionExecutedContext context)
        {
            context.CallContext.ResponseProperties.SetHeader(Key, Value);
        }
    }
}
