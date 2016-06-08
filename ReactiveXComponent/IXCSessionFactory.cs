using System.IO;

namespace ReactiveXComponent
{
    public interface IXCSessionFactory
    {
        IXCSession CreateSession(Stream file);
        void Close();
    }
}