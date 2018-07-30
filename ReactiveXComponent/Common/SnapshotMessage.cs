
using System;
using System.Collections.Generic;

namespace ReactiveXComponent.Common
{
    [Serializable]
    public class SnapshotMessage
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

        public string Filter { get; set; }
        public TimeSpan? Timeout { get; set; } = DefaultTimeout;
        public List<string> CallerPrivateTopic { get; set; }
        public string ReplyTopic { get; set; }
        public int? ChunkSize { get; set; }
    }
}
