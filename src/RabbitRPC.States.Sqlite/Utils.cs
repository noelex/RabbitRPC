using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.States.Sqlite
{
    internal static class Utils
    {
        public static IDisposable Rent<T>(this ArrayPool<T> pool, int length, out T[] buffer)
        {
            var r = pool.Rent(length);
            buffer = r;
            return new Disposable(() => pool.Return(r));
        }

        private class Disposable : IDisposable
        {
            private readonly Action _action;

            public Disposable(Action action) => _action = action;

            public void Dispose() => _action();
        }
    }
}
