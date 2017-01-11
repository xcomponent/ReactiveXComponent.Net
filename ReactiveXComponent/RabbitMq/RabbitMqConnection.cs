using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqConnection : IXCConnection
    {
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;
        private readonly BusDetails _busDetails;

        public RabbitMqConnection(IXCConfiguration configuration, string privateCommunicationIdentifier = null)
        {
            _xcConfiguration = configuration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _busDetails = configuration?.GetBusDetails();
        }

        public IXCSession CreateSession()
        {
            return new RabbitMqSession(_xcConfiguration, _busDetails, _privateCommunicationIdentifier);
        }
    }
}