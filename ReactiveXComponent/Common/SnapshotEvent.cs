using System;
using System.Collections.Generic;

namespace ReactiveXComponent.Common
{
    public class SnapshotEvent
    {
        public string RequestId { get; set; }

        public SnapshotResponse SnapshotResponse { get; set; }
    }
}
