using System;
using System.IO;
using ReactiveXComponent.Parser;

namespace ReactiveXComponent.Configuration
{
    public class XCConfiguration : IXCConfiguration
    {
        private readonly XCApiConfigParser _parser;

        public XCConfiguration(XCApiConfigParser parser)
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
                throw new Exception("Failed to init configuration", ex);
            }
        }

        public string GetConnectionType()
        {
            try
            {
                return _parser.GetConnectionType();
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Connection Type not found", ex);
            }            
        }

        public long GetStateMachineCode(string component, string stateMachine)
        {
            try
            {
                return _parser.GetStateMachineCode(component, stateMachine);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("State Machine code not found", ex);
            }
            
        }

        public long GetComponentCode(string component)
        {
            try
            {
                return _parser.GetComponentCode(component);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Component code not found", ex);
            }
        }

        public int GetPublisherEventCode(string evnt)
        {
            try
            {
                return _parser.GetPublisherEventCode(evnt);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Event code not found", ex);
            }
        }

        public BusDetails GetBusDetails()
        {
            try
            {
                return _parser.GetBusDetails();
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Bus details not found", ex);
            }  
        }

        public string GetPublisherTopic(string component, string stateMachine, int eventCode)
        {
            try
            {
                return _parser.GetPublisherTopic(component, stateMachine, eventCode);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Publisher topic not found", ex);
            }   
        }

        public string GetConsumerTopic(string component, string stateMachine, int eventCode)
        {
            try
            {
                return _parser.GetConsumerTopic(component, stateMachine);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("Consumer topic not found", ex);
            }
        }
    }
}
