﻿using System;
using System.IO;
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
            catch (InitConfigurationException ex)
            {
                throw new InitConfigurationException("Failed to init configuration", ex);
            }
        }

        public string GetConnectionType()
        {
            return _parser.GetConnectionType();   
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
            return _parser.GetPublisherTopic(component, stateMachine, eventCode);
        }

        public string GetSubscriberTopic(string component, string stateMachine)
        {
            return _parser.GetSubscriberTopic(component, stateMachine);
        }
    }
}