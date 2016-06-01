using NUnit.Framework;
using ReactiveXComponent.Parser;
using ReactiveXComponent.RabbitMQ;

namespace ReactiveXComponentTest.ParserTests
{
    [TestFixture]
    class DeploymentParserTest
    {
        [TestCase()]
        public void ParseXML_Success_test()
        {
            var expectedBusInfos = new BusDetails()
            {
                Username = "guest",
                Password = "guest", 
                Host = "127.0.0.1",
                Port = 5672
            };

            var parser = new DeploymentParser("ParserTests//helloworldApi.xcApi");

            var busInfos = parser.BusDetails;

            Assert.AreEqual(expectedBusInfos, busInfos);
        }

        [TestCase()]
        public void GetPublisherTopic_Sucess_Test()
        {
            var parser = new DeploymentParser("ParserTests//helloworldApi.xcApi");

            var component = "HelloWorld";
            var stateMachine = "HelloWorldManager";
            int eventCode = 9;

            var expectedTopic = "input.1_0.HelloMicroservice.HelloWorld.HelloWorldManager";
            var topic = parser.GetPublisherTopic(component, stateMachine, eventCode);

            Assert.AreEqual(expectedTopic, topic);
        }

        [TestCase()]
        public void GetConsumerTopic_Sucess_Test()
        {
            var parser = new DeploymentParser("ParserTests//helloworldApi.xcApi");

            var component = "HelloWorld";
            var stateMachine = "HelloResponse";

            var expectedTopic = "output.1_0.HelloMicroservice.HelloWorld.HelloResponse";
            var topic = parser.GetConsumerTopic(component, stateMachine);

            Assert.AreEqual(expectedTopic, topic);
        }

    }
}
