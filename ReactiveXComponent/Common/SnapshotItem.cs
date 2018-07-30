using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveXComponent.Common
{
    public class SnapshotItem
    {
        public const int DefaultIntValue = -1;
        public const string DefaultStringValue = "";

        public string StateMachineId { get; set; }

        public int WorkerId { get; set; }

        public int StateMachineCode { get; set; }

        public int StateCode { get; set; }

        public int ComponentCode { get; set; }

        public object PublicMember { get; set; }

        public SnapshotItem()
        {
            StateMachineId = DefaultStringValue;
            WorkerId = DefaultIntValue;
            StateMachineCode = DefaultIntValue;
            StateCode = DefaultIntValue;
            ComponentCode = DefaultIntValue;
            PublicMember = null;
        }
    }
}
