using System;
using System.IO;
using ReactiveXComponent.Parser;

namespace ReactiveXComponent.Connection
{
    public class XCPublisherFactory : IXCPublisherFactory
    {
        private readonly DeploymentParser _parser;

        public XCPublisherFactory(DeploymentParser parser)
        {
            _parser = parser;
        }

        public IXCPublisher CreatePublisher()
        {
            return new XCPublisher(_parser);
        }
    }
}
