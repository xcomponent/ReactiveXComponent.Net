using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveXComponent.Common
{
    public enum IncomingEventType
    {
        Transition = 0 ,
        X = 1,
        Stop = 2 ,
        Manager = 3 ,
        EntryPoint = 4 ,
        Init = 5 ,
        Test = 6 ,
        Dump = 7 ,
        InitTest = 8 ,
        Update = 9 ,
        Get = 10 ,
        Error = 11 ,
        Snapshot = 12
    }
}
