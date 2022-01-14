using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitRPC.Client.Filters
{
    interface IFilterChain
    {
        Task ExecuteAsync();
    }

    class RequestFilterChain : IFilterChain
    {
        private readonly IRequestFilter _filter;
        private readonly IFilterChain _next;
        private readonly IRequestStartingContext _executingContext;
        private readonly IRequestCompletedContext _executedContext;

        public RequestFilterChain(IFilterChain next, IRequestFilter filter, IRequestStartingContext actionExecutingContext, IRequestCompletedContext actionExecutedContext)
        {
            (_filter, _next, _executingContext, _executedContext) = (filter, next, actionExecutingContext, actionExecutedContext);
        }

        public async Task ExecuteAsync()
        {
            _filter.OnRequestStarting(_executingContext);

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

            _filter.OnRequestCompleted(_executedContext);
        }
    }

    class AsyncRequestFilterChain : IFilterChain
    {
        private readonly IAsyncRequestFilter _filter;
        private readonly IFilterChain _next;
        private readonly IRequestStartingContext _executingContext;
        private readonly IRequestCompletedContext _executedContext;

        public AsyncRequestFilterChain(IFilterChain next, IAsyncRequestFilter filter, IRequestStartingContext actionExecutingContext, IRequestCompletedContext actionExecutedContext)
        {
            (_filter, _next, _executingContext, _executedContext) = (filter, next, actionExecutingContext, actionExecutedContext);
        }

        public async Task ExecuteAsync()
        {
            await _filter.OnRequestInvocationAsync(_executingContext, new RequestExecutionDelegate(async () =>
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

    class PrepareRequestFilterChain : IFilterChain
    {
        private readonly IPrepareRequestFilter _initFilter;
        private readonly IFilterChain _next;
        private readonly IPrepareRequestContext _context;

        public PrepareRequestFilterChain(IFilterChain next, IPrepareRequestFilter serviceInstantiationFilter, IPrepareRequestContext context)
        {
            _initFilter = serviceInstantiationFilter;
            _next = next;
            _context = context;
        }

        public Task ExecuteAsync()
        {
            _initFilter.OnPrepareRequest(_context);
            return _next.ExecuteAsync();
        }
    }

    class ResponseReceivedFilterChain : IFilterChain
    {
        private readonly IResponseReceivedFilter _initFilter;
        private readonly IFilterChain _next;
        private readonly IResponseReceivedContext _context;

        public ResponseReceivedFilterChain(IFilterChain next, IResponseReceivedFilter filter, IResponseReceivedContext context)
        {
            _initFilter = filter;
            _next = next;
            _context = context;
        }

        public Task ExecuteAsync()
        {
            _initFilter.OnResponseReceived(_context);
            return _next.ExecuteAsync();
        }
    }

    class ChainTermination : IFilterChain
    {
        private readonly RequestExecutionDelegate _next;

        public ChainTermination(RequestExecutionDelegate next)
        {
            _next = next;
        }

        public Task ExecuteAsync() => _next();
    }
}
