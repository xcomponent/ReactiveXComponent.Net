using System;
using System.IO;

namespace ReactiveXComponent
{
    public interface IXComponentApi: IDisposable
    {
        XCSession CreateSession();
    }
}