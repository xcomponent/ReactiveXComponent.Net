using System;

namespace ReactiveXComponent.Common
{
    public class WebSocketMessageEventArgs : EventArgs
    {
        public WebSocketMessageEventArgs(string data, byte[] rawData)
        {
            Data = data;
            RawData = rawData;
        }

        public string Data { get; }
        public byte[] RawData { get; }
    }
}
