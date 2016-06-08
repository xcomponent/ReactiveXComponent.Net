using System;
using ReactiveXComponent.Common;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponent.XCApi
{
    public class XCSession : IXCSession
    {
        private bool _disposed = false;

        private XCPublisher _xcPublisher;
        private XCConsumer _xcConsumer;

        public XCPublisher XCPublisher => _xcPublisher;
        public XCConsumer XCConsumer => _xcConsumer;

        private readonly IRabbitMqPublisherFactory _publisherFactory;
        private readonly IRabbitMqConsumerFactory _consumerFactory;
        private readonly DeploymentParser _parser;

        public XCSession(IRabbitMqPublisherFactory publisherFactory, IRabbitMqConsumerFactory consumerFactory, DeploymentParser parser)
        {
            _publisherFactory = publisherFactory;
            _consumerFactory = consumerFactory;
            _parser = parser;
        }

        public void CreatePublisher(string component)
        {
            _xcPublisher = new XCPublisher(_publisherFactory, _parser);
            _xcPublisher.CreatePublisher(component);
        }
        public void CreateConsumer(string component, string stateMachine)
        {
            _xcConsumer = new XCConsumer(_consumerFactory, _parser);
            _xcConsumer.CreateConsummer(component, stateMachine);
        }

        public void Close()
        {
            _xcPublisher?.Close();
            _xcConsumer?.Close();
        }

        private void Dispose(bool disposed)
        {
            if (disposed)
                return;
            else
            {
                Close();
            }
            _disposed = true;
        }
        
        public void Dispose()
        {
            Dispose(_disposed);
            GC.SuppressFinalize(this);
        }
    }
}
