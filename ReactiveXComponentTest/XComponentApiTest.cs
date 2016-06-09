using System;
using System.IO;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using ReactiveXComponent;
using ReactiveXComponent.Connection;

namespace ReactiveXComponentTest
{
    [TestFixture]
    public class XComponentApiTest
    {
        private Stream _xcApiStream;

        [SetUp]
        public void Setup()
        {
            _xcApiStream = Stream.Null;   
        }

        [TestCase]
        public void CreateSession_GivenNoArgs_ShouldReturnAValidSession()
        {
            var xcApi = XComponentApi.CreateFromXCApi(_xcApiStream) as XComponentApi;
            using (xcApi)
            {
                var session = xcApi?.CreateSession();
                Check.That(session).IsInstanceOf<XCSession>();
                using (session)
                {
                    var publisher = session?.CreatePublisher(String.Empty);
                    Check.That(publisher).IsInstanceOf<XCPublisher>();
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            _xcApiStream?.Dispose();
        }
    }
}
