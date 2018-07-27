using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReactiveXComponent.Common
{
    public class ChunkCountdown : IDisposable
    {
        private readonly ManualResetEvent _completionResetEvent = new System.Threading.ManualResetEvent(false);

        private const int ValueNotSet = int.MinValue;
        private int _countdown = ValueNotSet;

        public void SetValueIfNotInitialized(int countdown)
        {
            Interlocked.CompareExchange(ref _countdown, countdown, ValueNotSet);
        }

        public void Decrement()
        {
            bool isCompleted = Interlocked.Decrement(ref _countdown) <= 0;
            if (isCompleted)
            {
                _completionResetEvent.Set();
            }
        }

        public WaitHandle CompletionResetEvent => _completionResetEvent;

        public int Countdown => _countdown;

        public void Dispose()
        {
            _completionResetEvent?.Dispose();
        }
    }
}
