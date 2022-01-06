using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitRPC.States.Sqlite
{
    internal class SqliteStateTransaction : ITransaction
    {
        private readonly IDbContextTransaction _underlyingTransaction;

        public SqliteStateTransaction(IDbContextTransaction underlyingTransaction)
        {
            _underlyingTransaction = underlyingTransaction;
        }

        public Task CommitAsync(CancellationToken cancellationToken)
        {
            return _underlyingTransaction.CommitAsync(cancellationToken);
        }

        public void Dispose()
        {
            _underlyingTransaction.Dispose();
        }

        public Task RollbackAsync(CancellationToken cancellationToken)
        {
            return _underlyingTransaction.RollbackAsync(cancellationToken);
        }
    }
}
