using System.IO;
using System.Net.Http.Internal;

namespace System.Net.Http.Mocks
{
    internal class MockDelegatingStream : DelegatingStream
    {
        public MockDelegatingStream(Stream innerStream)
            : base(innerStream)
        {
        }
    }
}
