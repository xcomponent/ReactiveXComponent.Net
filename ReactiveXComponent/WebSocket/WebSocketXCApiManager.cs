using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Newtonsoft.Json;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.WebSocket
{
    public class WebSocketXCApiManager : IDisposable
    {
        private readonly IWebSocketClient _webSocketClient;

        private Stream _xcApiFileStream;

        private event EventHandler<List<string>> XCApiNamesReceived;
        private event EventHandler<Stream> XCApiReceived;

        private readonly IObservable<List<string>> _xcApiNamesStream;
        private readonly IObservable<Stream> _xcApiStream;

        public WebSocketXCApiManager(IWebSocketClient webSocketClient)
        {
            _webSocketClient = webSocketClient;

            _xcApiNamesStream = Observable.FromEvent<EventHandler<List<string>>, List<string>>(
                handler => (sender, e) => handler(e),
                h => XCApiNamesReceived += h,
                h => XCApiNamesReceived -= h);

            _xcApiStream = Observable.FromEvent<EventHandler<Stream>, Stream>(
                handler => (sender, e) => handler(e),
                h => XCApiReceived += h,
                h => XCApiReceived -= h);
        }

        public List<string> GetXCApiNames(int timeout = 10000)
        {
            List<string> result = null;
            var lockEvent = new AutoResetEvent(false);
            var observer = Observer.Create<List<string>>(message =>
            {
                result = new List<string>(message);
                lockEvent.Set();
            });
            var xcApiSubscription = _xcApiNamesStream.Subscribe(observer);

            EventHandler<WebSocketMessageEventArgs> subscriptionHandler;
            
            SubscribeXCApiNames(out subscriptionHandler);
            lockEvent.WaitOne(timeout);
            
            xcApiSubscription.Dispose();
            _webSocketClient.MessageReceived -= subscriptionHandler;

            return result;
        }


        public Stream GetXCApi(string apiFullName, int timeout = 10000)
        {
            Stream result = null;
            var lockEvent = new AutoResetEvent(false);
            var observer = Observer.Create<Stream>(message =>
            {
                result = message;
                lockEvent.Set();
            });
            var xcApiSubscription = _xcApiStream.Subscribe(observer);

            EventHandler<WebSocketMessageEventArgs> subscriptionHandler;

            SubscribeXCApi(apiFullName, out subscriptionHandler);
            lockEvent.WaitOne(timeout);

            xcApiSubscription.Dispose();
            _webSocketClient.MessageReceived -= subscriptionHandler;

            return result;
        }

        private void SubscribeXCApiNames(out EventHandler<WebSocketMessageEventArgs> subscriptionHandler)
        {
            var webSocketRequest = WebSocketMessageHelper.SerializeXCApiRequest(WebSocketCommand.GetXCApiList);
            subscriptionHandler = (sender, args) =>
            {
                string rawRequest = args.Data;
                var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(rawRequest);
                var xcApiNames = new List<string>();
                if (webSocketMessage.Command == WebSocketCommand.GetXCApiList.ToString())
                {
                    var response = JsonConvert.DeserializeObject<WebSocketGetXcApiListResponse>(webSocketMessage.Json);
                    xcApiNames.AddRange(response.Apis);
                } 
                XCApiNamesReceived?.Invoke(this, xcApiNames); 
            };

            _webSocketClient.MessageReceived += subscriptionHandler;

            _webSocketClient.Send(webSocketRequest);
        }

        private void SubscribeXCApi(string apiFullName, out EventHandler<WebSocketMessageEventArgs> subscriptionHandler)
        {
            var apiNames = GetXCApiNames();
            if (apiNames.Contains(apiFullName))
            {
                var webSocketRequest = WebSocketMessageHelper.SerializeXCApiRequest(WebSocketCommand.GetXCApi, apiFullName);
                subscriptionHandler = (sender, args) =>
                {
                    string rawRequest = args.Data;
                    var webSocketMessage = WebSocketMessageHelper.DeserializeRequest(rawRequest);
                    _xcApiFileStream = null;
                    if (webSocketMessage.Command == WebSocketCommand.GetXCApi.ToString())
                    {
                        var response = JsonConvert.DeserializeObject<WebSocketGetXcApiResponse>(webSocketMessage.Json);
                        var xcApiContent = WebSocketMessageHelper.DeserializeXCApi(response.Content);
                        _xcApiFileStream = WebSocketMessageHelper.ToStream(xcApiContent);
                    }
                    XCApiReceived?.Invoke(this, _xcApiFileStream);
                };

                _webSocketClient.MessageReceived += subscriptionHandler;

                _webSocketClient.Send(webSocketRequest);
            }
            else
            {
                subscriptionHandler = null;
            }  
        }

        #region IDisposable implementation

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _xcApiFileStream.Dispose();
                }

                // clear unmanaged resources
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WebSocketXCApiManager()
        {
            Dispose(false);
        }

        #endregion
    }
}
