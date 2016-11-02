using System;

namespace ReactiveXComponent.Common
{
    [Serializable]
    public class StateMachineInstance
    {
        public long StateMachineId { get; set; }

        public long AgentId { get; set; }

        public long StateCode { get; set; }

        public long StateMachineCode { get; set; }

        public long ComponentCode { get; set; }

        public object PublicMember { get; set; }
     }
}
