namespace RabbitRPC.ServiceHost
{
    internal class ActionContext : IActionContext
    {
        public ActionContext() { }

        public ActionContext(IActionContext actionContext)
        {
            CallContext = actionContext.CallContext;
            ActionDescriptor = actionContext.ActionDescriptor;
            ServiceDescriptor = actionContext.ServiceDescriptor;
        }
        public virtual ActionDescriptor ActionDescriptor { get; set; } = null!;

        public virtual ServiceDescriptor ServiceDescriptor { get; set; } = null!;

        public virtual ICallContext CallContext { get; set; } = null!;
    }
}
