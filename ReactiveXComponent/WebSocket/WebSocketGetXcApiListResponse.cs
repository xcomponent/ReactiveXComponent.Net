using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketGetXcApiListResponse
    {
        public string RequestId { get; set; }

        public List<string> Apis { get; set; }
    }
}
