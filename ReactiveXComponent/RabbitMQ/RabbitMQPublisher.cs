namespace ReactiveXComponent.RabbitMQ
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    using global::RabbitMQ.Client;

    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private bool _disposed = false;

        private readonly IModel _publisherChannel;
        private readonly string _exchangeName;

        public RabbitMqPublisher(string exchangeName, IRabbitMqConnection rabbitMqConnection)
        {            
            _exchangeName = exchangeName;
            var connection = rabbitMqConnection.GetConnection();
            if (connection != null && connection.IsOpen)
            {
                _publisherChannel = connection.CreateModel();                
                _publisherChannel.ExchangeDeclare(_exchangeName, ExchangeType.Topic);                
            }
        }

        public void Send(Header header, object message, string routingKey)
        {            
            var prop = _publisherChannel.CreateBasicProperties();
            prop.Headers = RabbitMqHeaderConverter.ConvertHeader(header);

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
                Serialize(stream, message);
                messageBytes = stream.ToArray();
            }

            if (messageBytes == null)
                throw new Exception("Message serialisation failed");

            try
            {
                _publisherChannel.BasicPublish(_exchangeName, routingKey, properties, messageBytes);
            }            
            catch (Exception exception)
            {
                throw new Exception("The publication failed: " + exception.Message, exception);
            }
        }

        private void Serialize(Stream stream, object message)
        {
            if (message != null)
            {

                var binaryFormater = new BinaryFormatter();
                binaryFormater.Serialize(stream, message);                
            }
        }

        public void Close()
        {
            _publisherChannel.Close();            
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
