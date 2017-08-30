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

        public RabbitMqConnection(IXCConfiguration configuration, string privateCommunicationIdentifier = null)
        {
            _xcConfiguration = configuration;
            _privateCommunicationIdentifier = privateCommunicationIdentifier;
            _busDetails = configuration?.GetBusDetails();
        }

        public IXCSession CreateSession(TimeSpan? timeout = null, TimeSpan? retryInterval = null, int maxRetries = 5)
        {
            return new RabbitMqSession(_xcConfiguration, _busDetails, _privateCommunicationIdentifier, timeout, retryInterval);
        }
    }
}