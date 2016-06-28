using System;

namespace ReactiveXComponent.RabbitMq
{
    public class StringEventArgs : EventArgs
    {
        public StringEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}