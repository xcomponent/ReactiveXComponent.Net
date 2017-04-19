using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Newtonsoft.Json;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketXCApiManager
    {
        private readonly IWebSocketClient _webSocketClient;

        private event EventHandler<WebSocketGetXcApiListResponse> XCApiListReceived;
        private event EventHandler<WebSocketGetXcApiResponse> XCApiReceived;

        private readonly IObservable<WebSocketGetXcApiListResponse> _xcApiListStream;
        private readonly IObservable<WebSocketGetXcApiResponse> _xcApiStream;

        public WebSocketXCApiManager(IWebSocketClient webSocketClient)
        {
            _webSocketClient = webSocketClient;

            _xcApiListStream = Observable.FromEvent<EventHandler<WebSocketGetXcApiListResponse>, WebSocketGetXcApiListResponse>(
                handler => (sender, e) => handler(e),
                h => XCApiListReceived += h,
                h => XCApiListReceived -= h);

            _xcApiStream = Observable.FromEvent<EventHandler<WebSocketGetXcApiResponse>, WebSocketGetXcApiResponse>(
                handler => (sender, e) => handler(e),
                h => XCApiReceived += h,
                h => XCApiReceived -= h);
        }

        public List<string> GetXCApiList(string requestId = null, TimeSpan ? timeout = null)
        {
            var delay = timeout ?? TimeSpan.FromSeconds(10);
            List<string> result = new List<string>();
            var lockEvent = new AutoResetEvent(false);
            var observer = Observer.Create<WebSocketGetXcApiListResponse>(response =>
            {
                if (response.RequestId == requestId)
                {
                    result = new List<string>(response.Apis);
                    lockEvent.Set();
                }
            });
            var request = new WebSocketXCApiCommand
            {
                Command = WebSocketCommand.GetXCApiList,
                Id = requestId
            };

            using (_xcApiListStream.Subscribe(observer))
            {
                _webSocketClient.MessageReceived += ProcessResponse;
                SendRequest(request);
                lockEvent.WaitOne(delay);
                _webSocketClient.MessageReceived -= ProcessResponse;
            }

            return result;
        }


        public string GetXCApi(string apiFullName, string requestId = null, TimeSpan ? timeout = null)
        {
            var delay = timeout ?? TimeSpan.FromSeconds(10);
            var result = string.Empty;
            var lockEvent = new AutoResetEvent(false);
            var observer = Observer.Create<WebSocketGetXcApiResponse>(response =>
            {
                if (response.RequestId == requestId && response.ApiFound)
                {
                    result = response.ApiName;
                    lockEvent.Set();
                }
            });
            var request = new WebSocketXCApiCommand
            {
                Command = WebSocketCommand.GetXCApi,
                Id = requestId
            };

            using (_xcApiStream.Subscribe(observer))
            {
                _webSocketClient.MessageReceived += ProcessResponse;
                SendRequest(request, apiFullName);
                lockEvent.WaitOne(delay);
                _webSocketClient.MessageReceived -= ProcessResponse;
            }

            return result;
        }

        private void SendRequest(WebSocketXCApiCommand command, string apiFullName = null)
        {
            var webSocketRequest = WebSocketMessageHelper.SerializeXCApiRequest(command, apiFullName);
            _webSocketClient.Send(webSocketRequest);
        }

        private void ProcessResponse(object sender, WebSocketMessageEventArgs args)
        {
            var rawRequest = args.Data;
            var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(rawRequest);

            if (webSocketMessage.Command == WebSocketCommand.GetXCApiList)
            {
                var response = JsonConvert.DeserializeObject<WebSocketGetXcApiListResponse>(webSocketMessage.Json);
                XCApiListReceived?.Invoke(this, response);
            }
            else if (webSocketMessage.Command == WebSocketCommand.GetXCApi)
            {
                var response = JsonConvert.DeserializeObject<WebSocketGetXcApiResponse>(webSocketMessage.Json);
                XCApiReceived?.Invoke(this, response);
            }
        }
    }
}
