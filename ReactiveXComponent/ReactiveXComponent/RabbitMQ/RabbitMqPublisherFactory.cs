using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactiveXComponent.RabbitMQ
{
    using XComponent.Common.Helper;

    public class RabbitMqPublisherFactory : IRabbitMqPublisherFactory
    {
        private readonly IRabbitMqConnection connection;

        public RabbitMqPublisherFactory(IRabbitMqConnection connection)
        {
            this.connection = connection;
        }

        public IRabbitMQPublisher Create(string componentName)
        {
        
            return new RabbitMQPublisher(HashcodeHelper.GetXcHashCode(componentName).ToString(), this.connection);       
        }
    }
}
