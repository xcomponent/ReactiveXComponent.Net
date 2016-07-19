﻿using System;
using System.IO;
using NFluent;
using NUnit.Framework;
using ReactiveXComponent.Configuration;
using ReactiveXComponent.Parser;

namespace ReactiveXComponentTest.UnitTests.ParserTests
{
    [TestFixture]
    public class XCApiConfigParserTest : IDisposable
    {
        private bool _disposed;
        private string _component;
        private string _stateMachine;
        private XCApiConfigParser _xcApiConfigParser;
        private Stream _xcApiStream;
        
        [SetUp]
        public void Setup()
        {
            _component = "HelloWorld";
            _stateMachine = "HelloWorldManager";
            _xcApiStream = new FileStream("TestApi.xcApi", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
        }


        [Test]
        public void GetConnectionType_ShouldReturnConnectionTypeFromParsedStream()
        {
            _xcApiConfigParser = new XCApiConfigParser();
            _xcApiConfigParser.Parse(_xcApiStream);

            const string expectedConnectionType = "bus";
            var connectionType = _xcApiConfigParser.GetConnectionType();

            Check.That(connectionType).IsEqualTo(expectedConnectionType);
        }

        [Test]
        public void GetBusDetails_ShouldReturnBusInfosFromParsedStraem()
        {
            _xcApiConfigParser = new XCApiConfigParser();
            _xcApiConfigParser.Parse(_xcApiStream);

            var expectedBusDetails = new BusDetails
            {
                Username = "guest",
                Password = "guest",
                Host = "127.0.0.1",
                Port = 5672
            };
            var busDetails = _xcApiConfigParser.GetBusDetails();

            Check.That(busDetails).IsEqualTo(expectedBusDetails);
        } 

        [Test]
        public void GetComponentCode_GivenComponent_ShouldReturnTheComponentIddentifier_Test()
        {
            _xcApiConfigParser = new XCApiConfigParser();
            _xcApiConfigParser.Parse(_xcApiStream);

            const int expectedIdentifier = -69981087;
            var componentCode = _xcApiConfigParser.GetComponentCode(_component);

            Check.That(componentCode).IsEqualTo(expectedIdentifier);
        }

        [Test]
        public void GetStateMachineCode_GivenComponentAndStateMachine_ShouldReturnTheStateMachineIddentifier_Test()
        {
            _xcApiConfigParser = new XCApiConfigParser();
            _xcApiConfigParser.Parse(_xcApiStream);

            const int expectedIdentifier = -829536631;
            var stateMachineCode = _xcApiConfigParser.GetStateMachineCode(_component, _stateMachine);

            Check.That(stateMachineCode).IsEqualTo(expectedIdentifier);
        }

        [Test]
        public void GetGetPublisherEventCode_GivenAMessageType_ShouldReturnThePublisherEventCode_Test()
        {
            _xcApiConfigParser = new XCApiConfigParser();
            _xcApiConfigParser.Parse(_xcApiStream);

            const int expectedEventCode = 9;
            const string messageType = "XComponent.HelloWorld.UserObject.SayHello";
            var eventCode = _xcApiConfigParser.GetPublisherEventCode(messageType);

            Check.That(eventCode).IsEqualTo(expectedEventCode);
        }

        [Test]
        public void GetPublisherTopic_GiventComponentStateMachienAndEventCode_ShouldReturnAPublisherTopic_Test()
        {
            _xcApiConfigParser = new XCApiConfigParser();
            _xcApiConfigParser.Parse(_xcApiStream);

            const string messageType = "XComponent.HelloWorld.UserObject.SayHello";
            var eventCode = _xcApiConfigParser.GetPublisherEventCode(messageType);
            const string expectedTopic = "input.1_0.HelloMicroservice.HelloWorld.HelloWorldManager";
            var topic = _xcApiConfigParser.GetPublisherTopic(_component, _stateMachine, eventCode);

            Check.That(topic).IsEqualTo(expectedTopic);
        }

        [Test]
        public void GetConsumerTopic_GiventComponentAndStateMachien_ShouldReturnAConsumerTopic_Test()
        {
            _xcApiConfigParser = new XCApiConfigParser();
            _xcApiConfigParser.Parse(_xcApiStream);

            const string stateMachine = "HelloResponse";
            const string expectedTopic = "output.1_0.HelloMicroservice.HelloWorld.HelloResponse";
            var topic = _xcApiConfigParser.GetSubscriberTopic(_component, stateMachine);

            Check.That(topic).IsEqualTo(expectedTopic);
        }

        [Test]
        public void GetSerializationType_ShouldReturnSerializationType_Test()
        {
            _xcApiConfigParser = new XCApiConfigParser();
            _xcApiConfigParser.Parse(_xcApiStream);

            var expectedSerialization = "Binary";
            var serialization = _xcApiConfigParser.GetSerializationType();

            Check.That(serialization).IsEqualTo(expectedSerialization);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _xcApiConfigParser = null;
                _xcApiStream.Dispose();
            }
            _disposed = true;
        }

        [TearDown]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}