
using ReactiveXComponent.Common;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketSnapshotMessage
    {
        public long StateMachineCode { get; set; }
        public long ComponentCode { get; set; }
        public Option<string> ReplyTopic { get; set; }
        public Option<string[]> PrivateTopic { get; set; }

        public WebSocketSnapshotMessage() { }

        public WebSocketSnapshotMessage(long stateMachineCode, long componentCode, string replyTopic, string privateTopic)
        {
            StateMachineCode = stateMachineCode;
            ComponentCode = componentCode;
            ReplyTopic = new Option<string>(replyTopic);
            PrivateTopic = new Option<string[]>(new[] {privateTopic});
        }
    }
}
