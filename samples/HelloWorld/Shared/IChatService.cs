using RabbitRPC;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shared
{
    public interface IChatService : IRabbitService
    {
        Task<string> HelloAsync(string name, CancellationToken cancellationToken = default);
    }
}
