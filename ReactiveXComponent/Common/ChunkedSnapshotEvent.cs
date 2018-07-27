using System;

namespace ReactiveXComponent.Common
{
    public class ChunkedSnapshotEvent
    {
        public string RequestId { get; set; }

        public SnapshotResponseChunk SnapshotResponseChunk { get; set; }
    }
}
