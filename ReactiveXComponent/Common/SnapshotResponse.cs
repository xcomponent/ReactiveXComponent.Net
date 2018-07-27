
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ReactiveXComponent.Common
{
    [Serializable]
    public class SnapshotResponse
    {
        [JsonConverter(typeof(GZipSnapshotItemArrayJsonConverter<List<SnapshotItem>>))]
        public List<SnapshotItem> Items { get; set; }

        public SnapshotResponse()
        {
            Items = new List<SnapshotItem>();
        }
    }
}
