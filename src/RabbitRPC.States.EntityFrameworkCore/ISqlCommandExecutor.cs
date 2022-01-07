using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.States.EntityFrameworkCore
{
    public interface ISqlCommandExecutor
    {
        Task<int> AddOrUpdateAsync(DbContext dbContext, string key, byte[] value, long version, CancellationToken cancellationToken);

        Task<int> AddOrUpdateAsync(DbContext dbContext, string key, byte[] value, CancellationToken cancellationToken);

        Task<int> DeleteAsync(DbContext dbContext, string key, long version, CancellationToken cancellationToken);

        Task<int> DeleteAsync(DbContext dbContext, string key, CancellationToken cancellationToken);
    }
}
