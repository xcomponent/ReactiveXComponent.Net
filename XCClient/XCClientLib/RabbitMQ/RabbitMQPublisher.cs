namespace XCClientLib.RabbitMQ
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    using global::RabbitMQ.Client;

    public interface IRabbitMQPublisher : IDisposable
    {
        void Send(Header header, object message, string routingKey);

        void Close();
    }

    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private IModel publisherChannel;
        private readonly string exchangeName;        

        public RabbitMQPublisher(string exchangeName, IRabbitMqConnection rabbitMqConnection)
        {            
            this.exchangeName = exchangeName;
            var connection = rabbitMqConnection.GetConnection();
            if (connection != null && connection.IsOpen)
            {
                this.publisherChannel = connection.CreateModel();                
                this.publisherChannel.ExchangeDeclare(this.exchangeName, global::RabbitMQ.Client.ExchangeType.Topic);                
            }
        }

        public void Send(Header header, object message, string routingKey)
        {            
            var prop = this.publisherChannel.CreateBasicProperties();
            prop.Headers = RabbitMQHeaderConverter.ConvertHeader(header);

            if (message == null)
            {
                message = 0;
            }

            this.Send(message, routingKey, prop);
        }

        private void Send(object message, string routingKey, IBasicProperties properties)
        {
            byte[] messageBytes;
            using (var stream = new MemoryStream())
            {
                this.Serialize(stream, message);
                messageBytes = stream.ToArray();
            }

            if (messageBytes == null)
                throw new Exception("Message serialisation failed");

            try
            {
                this.publisherChannel.BasicPublish(this.exchangeName, routingKey, properties, messageBytes);
            }            
            catch (Exception exception)
            {
                throw new Exception("The publication failed: " + exception.Message, exception);
            }
        }

        public void Serialize(Stream stream, object message)
        {
            if (message != null)
            {

                var binaryFormater = new BinaryFormatter();
                binaryFormater.Serialize(stream, message);                
            }
        }

        public void Close()
        {
            this.publisherChannel.Close();            
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
