using RabbitRPC;

namespace Shared
{
    [RabbitService(Name = "CounterService")]
    public interface ICounterService:IRabbitService
    {
        [Action(Name = "Increment")]
        Task<long> IncrementAsync(CancellationToken cancellationToken = default);

        [Action(Name = "Decrement")]
        Task<long> DecrementAsync(CancellationToken cancellationToken = default);

        [Action(Name = "GetCounter")]
        Task<long> GetCounterAsync(CancellationToken cancellationToken = default);
    }
}