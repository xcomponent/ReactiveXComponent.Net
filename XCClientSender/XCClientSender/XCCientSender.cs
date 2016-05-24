using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCClientSender
{
    partial class XCClientSender: IDisposable
    {
        private string _xmlFile;

        public XCClientSender(string xmlFile)
        {
            _xmlFile = xmlFile;
        }
        public void Dispose()
        {}
    }
}
