
using ReactiveXComponent.Common;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketSnapshotMessage
    {
        public long StateMachineCode { get; set; }
        public long ComponentCode { get; set; }
        public string ReplyTopic { get; set; }
        public string[] CallerPrivateTopic { get; set; }
        public int? ChunkSize { get; set; }

        public WebSocketSnapshotMessage() { }

        public WebSocketSnapshotMessage(long stateMachineCode, long componentCode, string replyTopic, string callerPrivateTopic, int? chunkSize)
        {
            StateMachineCode = stateMachineCode;
            ComponentCode = componentCode;
            ReplyTopic = replyTopic;
            if (!string.IsNullOrEmpty(callerPrivateTopic))
            {
                CallerPrivateTopic = new []{ callerPrivateTopic };
            }
            ChunkSize = chunkSize;
        }
    }
}
