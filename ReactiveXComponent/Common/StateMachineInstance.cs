using System;

namespace ReactiveXComponent.Common
{
    [Serializable]
    public class StateMachineInstance
    {
        public long StateMachineId { get; set; }

        public int AgentId { get; set; }

        public int StateCode { get; set; }

        public int StateMachineCode { get; set; }

        public int ComponentCode { get; set; }

        public object PublicMember { get; set; }
     }
}
