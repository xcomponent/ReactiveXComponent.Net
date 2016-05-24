namespace XCClientLib.Builder
{
    using XCClientLib.Parser;
    using XCClientLib.RabbitMQ;

    public class XComponentBuilder
    {        
        private readonly DeploymentParser parser;

        private readonly object lockObject = new object();

        private IRabbitMqConnection rabbitMqConnection;

        public XComponentBuilder(string file)
        {
            this.parser = new DeploymentParser(file);            
        }

        public XComponentBuilder(string file, string host, int port)
            :this(file)
        {
            this.parser.Host = host;
            this.parser.Port = port;
        }

        public IXComponentApi CreateApi()
        {
            return new XComponentApi(this.CreatePublisherFactory(), this.CreateConsumerFactory(), this.CreateConnection(), this.parser);
        }

        private IRabbitMqConnection CreateConnection()
        {
            lock (this.lockObject)
            {
                if (this.rabbitMqConnection == null)
                {
                    return new RabbitMqConnection(this.parser.BusDetails);
                }

                return this.rabbitMqConnection;
            }
        }

        private IRabbitMqPublisherFactory CreatePublisherFactory()
        {
            lock (this.lockObject)
            {
                if (this.rabbitMqConnection == null)
                {
                    this.rabbitMqConnection = this.CreateConnection();
                }

                return new RabbitMqPublisherFactory(this.rabbitMqConnection);
            }
        }

        private IRabbitMQConsumerFactory CreateConsumerFactory()
        {
            lock (this.lockObject)
            {
                if (this.rabbitMqConnection == null)
                {
                    this.rabbitMqConnection = this.CreateConnection();
                }

                return new SingleKeyRabbitMQConsumerFactory(this.rabbitMqConnection);
            }
        }                        
    }
}
