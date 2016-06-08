using System.IO;

namespace ReactiveXComponent
{
    public class XCSessionFactory : IXCSessionFactory
    {
        public XCSessionFactory()
        {
            
        }

        public IXCSession CreateSession(Stream file)
        {
            return new XCSession();
        }

        public void Close()
        {
        }
    }
}