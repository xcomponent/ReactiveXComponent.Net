using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReactiveXComponent.Common;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.RabbitMq;

namespace ReactiveXComponentTest.IntegrationTests
{
    [TestFixture]
    [Category("Unit Tests")]
    public class RabbitMqCommunicationTest
    {
        private IXCConfiguration _xcConfiguration;
        private BusDetails _busDetails;
        private string _exchangeName;
        private string _routinKey;

        [SetUp]
        public void Setup()
        {
            _busDetails = new BusDetails
            {
                Host = "127.0.0.1",
                Password = "guest",
                Port = 5672,
                Username = "guest"
            };
            _exchangeName = "3";
            _routinKey = "Component.Test.Envoie.Message";
            _xcConfiguration = Substitute.For<IXCConfiguration>();
            _xcConfiguration.GetBusDetails().Returns(_busDetails);
            _xcConfiguration.GetComponentCode(null).ReturnsForAnyArgs(Convert.ToInt32(_exchangeName));
            _xcConfiguration.GetPublisherTopic(null, null, 0).ReturnsForAnyArgs(_routinKey);
        }

        [Test]
        public void SendEvent_GivenAStateMachineAMessageAndAVisibility_ShouldSendMessageToRabbitMq()
        {
            //Init RabbitMq Connection and Session
            var rabbitMqConnection = new RabbitMqConnection(_xcConfiguration);
            var session = rabbitMqConnection.CreateSession();
            
            var message = "J'envoie un message sur RabbitMq" as object;

            /*Spy intercepting any message on the same
            routingKey as Publisher*/
            var factory = new ConnectionFactory
            {
                UserName = _busDetails.Username,
                Password = _busDetails.Password,
                VirtualHost = ConnectionFactory.DefaultVHost,
                HostName = _busDetails.Host,
                Port = _busDetails.Port,
                Protocol = Protocols.DefaultProtocol
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic);
                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName,
                        exchange: _exchangeName,
                        routingKey: _routinKey);

                var consumer = new EventingBasicConsumer(channel);
                byte[] msg = new byte[] {};
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    msg = body;
                };
                channel.BasicConsume(queue: queueName, noAck: true, consumer: consumer);
                
                using (var publisher = session?.CreatePublisher("Component"))
                {
                    publisher?.SendEvent("stateMachine", message, Visibility.Public);
                }

                var binaryFormater = new BinaryFormatter();
                var obj = binaryFormater.Deserialize(new MemoryStream(msg));
                var contenu = obj.ToString();
                Check.That(contenu).Contains(message.ToString());
            }
        }
    }
}
