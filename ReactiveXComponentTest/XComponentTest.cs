using System.IO;
using System.Net.Http;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using ReactiveXComponent;

namespace ReactiveXComponentTest
{
    [TestFixture]
    public class XComponentTest
    {
        private IXCSessionFactory _sessionFactory;
        private XComponentApi _xcApi;
        private Stream _file;

        [SetUp]
        public void Setup()
        {
            _sessionFactory = Substitute.For<IXCSessionFactory>();
            _file = Stream.Null;
            _xcApi = new XComponentApi(_file);
        }

        [TestCase]
        public void TestCase()
        {
            using (_xcApi)
            {
                var session = _xcApi.CreateSession();

                Check.That(session).IsInstanceOf<XCSession>();
            }
        }

        [TearDown]
        public void TearDown()
        {
            _sessionFactory?.Close();
            _xcApi?.Dispose();
            _file?.Close();
        }
    }
}
