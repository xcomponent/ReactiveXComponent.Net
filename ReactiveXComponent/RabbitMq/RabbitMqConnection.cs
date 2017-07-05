using System;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent.RabbitMq
{
    public class RabbitMqConnection : IXCConnection
    {
        private readonly IXCConfiguration _xcConfiguration;
        private readonly string _privateCommunicationIdentifier;
        private readonly BusDetails _busDetails;
        private readonly TimeSpan? _retryInterval;

        public RabbitMqConnection(IXCConfiguration configuration, string privateCommunicationIdentifier = null, TimeSpan? retryInterval = null)
        {
            _xcConfiguration = configuration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _busDetails = configuration?.GetBusDetails();
            _retryInterval = retryInterval;
        }

        public IXCSession CreateSession()
        {
            return new RabbitMqSession(_xcConfiguration, _busDetails, _privateCommunicationIdentifier, _retryInterval);
        }
    }
}