using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitRPC.States
{
    internal class ActionDisposable : IDisposable
    {
        private Action _action;
        private bool _disposed;

        public ActionDisposable(Action action) => _action = action;

        public bool IsReleased => _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _action();
                _disposed = true;
            }
        }
    }
}
