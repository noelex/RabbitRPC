using RabbitRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Shared
{
    [RabbitService(Name = "ChatService")]
    public interface IChatService : IRabbitService
    {
        [Action(Name = "Hello")]
        Task<string> HelloAsync(string name, CancellationToken cancellationToken = default);
    }
}
