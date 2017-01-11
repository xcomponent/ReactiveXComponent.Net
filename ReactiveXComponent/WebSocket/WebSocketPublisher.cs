using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveXComponent.Common;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketPublisher : IXCPublisher
    {
        public void SendEvent(string stateMachine, object message, Visibility visibility = Visibility.Public)
        {
            throw new NotImplementedException();
        }

        public void SendEvent(StateMachineRefHeader stateMachineRefHeader, object message, Visibility visibility = Visibility.Public)
        {
            throw new NotImplementedException();
        }

        public List<MessageEventArgs> GetSnapshot(string stateMachine, int timeout = 10000)
        {
            throw new NotImplementedException();
        }

        public void GetSnapshotAsync(string stateMachine, Action<List<MessageEventArgs>> onSnapshotReceived)
        {
            throw new NotImplementedException();
        }


        #region IDisposable implementation

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // clear managed resources
                }

                // clear unmanaged resources

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WebSocketPublisher()
        {
            Dispose(false);
        }

        #endregion

    }
}
