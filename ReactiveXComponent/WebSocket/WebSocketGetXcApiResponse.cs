using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketGetXcApiResponse
    {
        public string RequestId { get; set; }

        public bool ApiFound { get; set; }

        public string ApiName { get; set; }

        public string Content { get; set; }
    }
}
