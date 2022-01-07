using RabbitRPC.States;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RabbitRPC.ServiceHost.Filters
{
    /// <summary>
    /// Re-execute the action when <see cref="ConcurrencyException"/> is thrown.
    /// </summary>
    /// <remarks>
    /// This filter should only be used when the concurrency token is retrieved inside the action.
    /// If the token is passed to the action via action parameters or read from cache, attempts to retry will never succeed.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RetryOnConcurrencyErrorAttribute : ActionFilterAttribute
    {
        private readonly Random _random = new Random();

        public int MaximumRetries { get; set; } = 10;

        /// <summary>
        /// Maximum backoff time (ms).
        /// </summary>
        public int MaximumBackoffTime { get; set; } = 10_000;

        /// <summary>
        /// Maximum jitter delay time (ms).
        /// </summary>
        public int MaximumJitter { get; set; } = 1000;

        /// <summary>
        /// Scale factor (ms) applied to the exponential backoff value before adding jitter delay time.
        /// </summary>
        public int ScaleFactor { get; set; } = 100;

        public override async Task OnActionExecutionAsync(IActionExecutingContext context, ActionExecutionDelegate next)
        {
            var result = await next();

            if (result.Exception is ConcurrencyException && context.CallContext.ExecutionId <= MaximumRetries)
            {
                result.UnhandledExceptionHandlingStrategy = ExceptionHandlingStrategy.ReExecute;
                await Task.Delay(CalculateBackoffTime(context.CallContext.ExecutionId), context.CallContext.RequestAborted);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CalculateBackoffTime(int n) =>  Math.Min((int)Math.Pow(2, n - 1) * ScaleFactor + _random.Next(MaximumJitter), MaximumBackoffTime);
    }
}
