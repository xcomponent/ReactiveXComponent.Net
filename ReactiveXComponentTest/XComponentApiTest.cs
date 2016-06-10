using System.IO;
using System.Xml.Linq;
using NFluent;
using NSubstitute;
using NUnit.Framework;
using ReactiveXComponent;
using ReactiveXComponent.Connection;
using ReactiveXComponentTest.ParserTests;

namespace ReactiveXComponentTest
{
    [TestFixture]
    [Category("Unit Tests")]
    public class XComponentApiTest : XCTestBase 
    {
        [Test]
        public void CreateFromXCApi_GivenXCApiStreamFile_ShouldReturnNewInstanceOfXCApi()
        {
            var xcApi = XComponentApi.CreateFromXCApi(XCApiStream);
            Check.That(xcApi).IsInstanceOf<XComponentApi>();
        }

        [Test]
        public void CreateSession_GivenXCApiStreamFile_ShouldReturnNewInstanceSession()
        {
            var xcApi = XComponentApi.CreateFromXCApi(XCApiStream) as XComponentApi;
            var session = xcApi?.CreateSession();
            Check.That(session).IsInstanceOf<XCSession>();
        }
    }
}
