using RabbitRPC;

namespace Sample.Shared
{
    public interface ITestService : IRabbitService
    {
        Task<string> HelloAsync(string name, CancellationToken cancellationToken = default);

        Task TestExceptionAsync(CancellationToken cancellationToken = default);

        Task TestTimeoutAsync(CancellationToken cancellationToken = default);

        Task<long> AddAsync(int a, long b, CancellationToken cancellationToken = default);

        Task<TestType> GetEnumAsync(CancellationToken cancellationToken = default);

        Task SetEnumAsync(TestType e, CancellationToken cancellationToken = default);

        Task<long> IncrementAsync(int delay, CancellationToken cancellationToken = default);

        Task<long> DecrementAsync(int delay, CancellationToken cancellationToken = default);

        Task<long> GetCounterAsync(CancellationToken cancellationToken = default);
    }

    public enum TestType
    {
        Default,
        Small,
        Medium,
        Large
    }

    public class ServerEvent
    {

    }

    public class ClientEvent
    {

    }

    public class PrintJob
    {
        public PrintJob(string text) => Text = text;

        public string Text { get; }
    }
}