using System;
using System.IO;
using NUnit.Framework;

namespace ReactiveXComponentTest
{
    public abstract class XCTestBase : IDisposable
    {
        protected Stream XCApiStream;

        [SetUp]
        public void PrivateSetup()
        {
            XCApiStream = new FileStream("TestApi.xcApi", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            Setup();
        }

        protected virtual void Setup() { }

        [TearDown]
        public void Dispose()
        {
            XCApiStream?.Dispose();
        }
    }
}