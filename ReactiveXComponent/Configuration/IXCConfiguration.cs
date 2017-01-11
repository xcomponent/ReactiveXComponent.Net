﻿using System.IO;
using ReactiveXComponent.Common;

namespace ReactiveXComponent.Configuration
{
    public interface IXCConfiguration
    {
        void Init(Stream xcApiStream);
        ConnectionType GetConnectionType();
        // TODO: replace these two methods with a GetCommunicationInfo() method
        BusDetails GetBusDetails();
        WebSocketEndpoint GetWebSocketEndpoint();
        long GetStateMachineCode(string component, string stateMachine);
        long GetComponentCode(string component);
        int GetPublisherEventCode(string evnt);
        string GetPublisherTopic(string component, string stateMachine);
        string GetPublisherTopic(long componentCode, long stateMachineCode);
        string GetSubscriberTopic(string component, string stateMachine);
        string GetSerializationType();
        string GetSnapshotTopic(string component);
    }
}