using System.IO;
using ReactiveXComponent.Connection;

namespace ReactiveXComponent
{
    public class XComponentApi : IXComponentApi
    {
        private readonly IXCConnection _xcConnection;       

        private XComponentApi(Stream xcApiStream)
        {
            _xcConnection = XCConnection.CreateConnection(xcApiStream);
        }

        public static IXComponentApi CreateFromXCApi(Stream xcApiStream)
        {
            return new XComponentApi(xcApiStream);
        }

        public IXCSession CreateSession()
        {
            return _xcConnection.CreateSession();
        }
    }
}
