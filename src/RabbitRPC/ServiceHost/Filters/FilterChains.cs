using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitRPC.ServiceHost.Filters
{
    interface IFilterChain
    {
        Task ExecuteAsync();
    }

    class ActionFilterChain : IFilterChain
    {
        private readonly IActionFilter _filter;
        private readonly IFilterChain _next;
        private readonly IActionExecutingContext _executingContext;
        private readonly IActionExecutedContext _executedContext;

        public ActionFilterChain(IFilterChain next, IActionFilter filter, IActionExecutingContext actionExecutingContext, IActionExecutedContext actionExecutedContext)
        {
            (_filter, _next, _executingContext, _executedContext) = (filter, next, actionExecutingContext, actionExecutedContext);
        }

        public async Task ExecuteAsync()
        {
            _filter.OnActionExecuting(_executingContext);

            if (_executingContext.Result == null && _executingContext.Exception == null)
            {
                await _next.ExecuteAsync();
            }
            else
            {
                _executedContext.Canceled = true;
                _executedContext.Result = _executingContext.Result;
                _executedContext.Exception = _executingContext.Exception;
            }

            _filter.OnActionExecuted(_executedContext);
        }
    }

    class AsyncActionFilterChain : IFilterChain
    {
        private readonly IAsyncActionFilter _filter;
        private readonly IFilterChain _next;
        private readonly IActionExecutingContext _executingContext;
        private readonly IActionExecutedContext _executedContext;

        public AsyncActionFilterChain(IFilterChain next, IAsyncActionFilter filter, IActionExecutingContext actionExecutingContext, IActionExecutedContext actionExecutedContext)
        {
            (_filter, _next, _executingContext, _executedContext) = (filter, next, actionExecutingContext, actionExecutedContext);
        }

        public async Task ExecuteAsync()
        {
            await _filter.OnActionExecutionAsync(_executingContext, new ActionExecutionDelegate(async () =>
            {
                if (_executingContext.Result == null && _executingContext.Exception == null)
                {
                    await _next.ExecuteAsync();
                }
                else
                {
                    _executedContext.Canceled = true;
                    _executedContext.Result = _executingContext.Result;
                    _executedContext.Exception = _executingContext.Exception;
                }

                return _executedContext;
            }));
        }
    }

    class ServiceInitializationFilterChain : IFilterChain
    {
        private readonly IServiceInitializationFilter _initFilter;
        private readonly IFilterChain _next;
        private readonly IActionContext _actionContext;
        private readonly IRabbitService _instance;

        public ServiceInitializationFilterChain(IFilterChain next, IServiceInitializationFilter serviceInstantiationFilter, IActionContext actionContext, IRabbitService instance)
        {
            _initFilter = serviceInstantiationFilter;
            _next = next;
            _instance = instance;
            _actionContext = actionContext;
        }

        public Task ExecuteAsync()
        {
            _initFilter.OnInitializeServiceInstance(_actionContext, _instance);
            return _next.ExecuteAsync();
        }
    }

    class ParameterBindingFilterChain : IFilterChain
    {
        private readonly IParameterBindingFilter _initFilter;
        private readonly IFilterChain _next;
        private readonly IActionContext _actionContext;
        private readonly IDictionary<string, object?> _parameters;

        public ParameterBindingFilterChain(IFilterChain next, IParameterBindingFilter filter, IActionContext actionContext, IDictionary<string, object?> parameters)
        {
            _initFilter = filter;
            _next = next;
            _parameters = parameters;
            _actionContext = actionContext;
        }

        public Task ExecuteAsync()
        {
            _initFilter.OnBindParameters(_actionContext, _parameters);
            return _next.ExecuteAsync();
        }
    }

    class ChainTermination : IFilterChain
    {
        private readonly ActionExecutionDelegate _next;

        public ChainTermination(ActionExecutionDelegate next)
        {
            _next = next;
        }

        public Task ExecuteAsync() => _next();
    }
}
