
using System;

namespace ReactiveXComponent.Common
{
    [Serializable]
    public class SnapshotMessage
    {
        public long StateMachineCode;
        public long ComponentCode;
        public ReplyTopic ReplyTopic;
        public PrivateTopic PrivateTopic;
    }
}
