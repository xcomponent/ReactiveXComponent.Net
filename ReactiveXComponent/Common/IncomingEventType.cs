using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveXComponent.Common
{
    public enum IncomingEventType
    {
        Transition,
        Stop,
        Manager,
        Init,
        Dump,
        InitTest,
        Update,
        Get,
        Error,
        Snapshot
    }
}
