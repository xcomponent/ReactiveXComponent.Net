using System;
using System.Collections.Generic;

namespace ReactiveXComponent.Common
{
    public class SnapshotResponseChunk
    {
        public SnapshotResponseChunk()
        {
        }

        public SnapshotResponseChunk(string runtimeId, List<string> knownRuntimeIds)
        {
            RuntimeId = runtimeId;
            KnownRuntimeIds = knownRuntimeIds;
        }

        public int ChunkCount { get; set; }

        public int ChunkId { get; set; }

        public string RuntimeId { get; set; }

        public List<string> KnownRuntimeIds { get; set; }

        public SnapshotResponse Response { get; set; }
    }
}
