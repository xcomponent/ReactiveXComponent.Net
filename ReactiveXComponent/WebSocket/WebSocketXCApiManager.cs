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

        private event EventHandler<List<string>> XCApiListReceived;
        private event EventHandler<string> XCApiReceived;

        private readonly IObservable<List<string>> _xcApiListStream;
        private readonly IObservable<string> _xcApiStream;

        public WebSocketXCApiManager(IWebSocketClient webSocketClient)
        {
            _webSocketClient = webSocketClient;

            _xcApiListStream = Observable.FromEvent<EventHandler<List<string>>, List<string>>(
                handler => (sender, e) => handler(e),
                h => XCApiListReceived += h,
                h => XCApiListReceived -= h);

            _xcApiStream = Observable.FromEvent<EventHandler<string>, string>(
                handler => (sender, e) => handler(e),
                h => XCApiReceived += h,
                h => XCApiReceived -= h);
        }

        public List<string> GetXCApiList(TimeSpan? timeout = null)
        {
            List<string> result = null;
            var lockEvent = new AutoResetEvent(false);
            var observer = Observer.Create<List<string>>(message =>
            {
                result = new List<string>(message);
                lockEvent.Set();
            });
            var xcApiSubscription = _xcApiListStream.Subscribe(observer);

            _webSocketClient.MessageReceived += ProcessResponse;
            SendRequest(WebSocketCommand.GetXCApiList);
            var delay = timeout ?? TimeSpan.FromSeconds(10);
            lockEvent.WaitOne(delay);
            
            xcApiSubscription.Dispose();
            _webSocketClient.MessageReceived -= ProcessResponse;

            return result;
        }


        public string GetXCApi(string apiFullName, TimeSpan? timeout = null)
        {
            var result = string.Empty;
            var lockEvent = new AutoResetEvent(false);
            var observer = Observer.Create<string>(message =>
            {
                result = message;
                lockEvent.Set();
            });
            var xcApiSubscription = _xcApiStream.Subscribe(observer);

            _webSocketClient.MessageReceived += ProcessResponse;
            SendRequest(WebSocketCommand.GetXCApi, apiFullName);
            var delay = timeout ?? TimeSpan.FromSeconds(10);
            lockEvent.WaitOne(delay);

            xcApiSubscription.Dispose();
            _webSocketClient.MessageReceived -= ProcessResponse;

            return result;
        }

        private void SendRequest(string requestCommand, string apiFullName = null)
        {
            var webSocketRequest = WebSocketMessageHelper.SerializeXCApiRequest(requestCommand, apiFullName);
            _webSocketClient.Send(webSocketRequest);
        }

        private void ProcessResponse(object sender, WebSocketMessageEventArgs args)
        {
            var rawRequest = args.Data;
            var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(rawRequest);

            if (webSocketMessage.Command == WebSocketCommand.GetXCApiList)
            {
                var response = JsonConvert.DeserializeObject<WebSocketGetXcApiListResponse>(webSocketMessage.Json);
                var xcApiNames = new List<string>(response.Apis);
                XCApiListReceived?.Invoke(this, xcApiNames);
            }
            else if (webSocketMessage.Command == WebSocketCommand.GetXCApi)
            {
                var response = JsonConvert.DeserializeObject<WebSocketGetXcApiResponse>(webSocketMessage.Json);
                if (response.ApiFound)
                {
                    var xcApiContent = WebSocketMessageHelper.DeserializeXCApi(response.Content);
                    XCApiReceived?.Invoke(this, xcApiContent);
                }
            }
        }
    }
}
