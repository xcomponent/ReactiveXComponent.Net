namespace XCClientLib.RabbitMQ
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    using global::RabbitMQ.Client.Events;

    public class SingleKeyRabbitMqConsumer : AbstractRabbitMQConsumer
    {
        private readonly string routingKey;        

        public SingleKeyRabbitMqConsumer(string exchangeName, string routingKey, IRabbitMqConnection rabbitMqConnection)
            : base(exchangeName, rabbitMqConnection)
        {
            this.routingKey = routingKey;
        }        

        protected override void SpecificStart()
        {
            this.Channel.QueueBind(this.QueueName, this.ExchangeName, this.routingKey, null);
        }        

        protected override void DispatchMessage(BasicDeliverEventArgs e)
        {
            var binaryFormater = new BinaryFormatter();

            var obj = binaryFormater.Deserialize(new MemoryStream(e.Body));
            var msgEventArgs = new MessageEventArgs(
                RabbitMQHeaderConverter.ConvertHeader(e.BasicProperties.Headers),
                obj);
            this.OnMessageReceived(msgEventArgs);         
        }
    }
}
