using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;
using XComponentClientApi;

namespace XComponentClientApiTest
{
    [TestFixture]
    public class XCCLientApiGenratorTests
    {
        [Test]
        public void Test()
        {
            XCClientApiGenerator.ExposeApi("PerseusApi.xcApi", "PerseusClientApi.cs");
        }
    }
}
