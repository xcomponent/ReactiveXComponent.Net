using System;
using System.IO;
using ReactiveXComponent.Common;
using ReactiveXComponent.Parser;

namespace ReactiveXComponent.Configuration
{
    public class XCConfiguration : IXCConfiguration
    {
        private readonly IXCApiConfigParser _parser;

        public XCConfiguration(IXCApiConfigParser parser)
        {
            _parser = parser;
        }

        public void Init(Stream xcApiStream)
        {
            try
            {
                _parser.Parse(xcApiStream);
            }
            catch (Exception ex)
            {
                throw new ReactiveXComponentException("Failed to init configuration", ex);
            }
        }

        public ConnectionType GetConnectionType()
        {
            var type = _parser.GetConnectionType();

            if (type == "bus")
            {
                return ConnectionType.RabbitMq;
            }

            if (type == "websocket")
            {
                return ConnectionType.WebSocket;
            }

            throw new ReactiveXComponentException($"Unrecognized connection type: {type}");
        }

        public string GetSerializationType()
        {
            return _parser.GetSerializationType();
        }

        public long GetStateMachineCode(string component, string stateMachine)
        {
            return _parser.GetStateMachineCode(component, stateMachine);
        }

        public long GetComponentCode(string component)
        {
            return _parser.GetComponentCode(component);
        }

        public int GetPublisherEventCode(string evnt)
        {
            return _parser.GetPublisherEventCode(evnt);
        }

        public BusDetails GetBusDetails()
        {
            return _parser.GetBusDetails();
        }

        public WebSocketEndpoint GetWebSocketEndpoint()
        {
            return _parser.GetWebSocketEndpoint();
        }

        public string GetPublisherTopic(string component, string stateMachine)
        {
            return _parser.GetPublisherTopic(component, stateMachine);
        }

        public string GetPublisherTopic(long componentCode, long stateMachineCode)
        {
            return _parser.GetPublisherTopic(componentCode, stateMachineCode);
        }

        public string GetSubscriberTopic(string component, string stateMachine)
        {
            return _parser.GetSubscriberTopic(component, stateMachine);
        }

        public string GetSnapshotTopic(string component)
        {
            return _parser.GetSnapshotTopic(component);
        }
    }
}
