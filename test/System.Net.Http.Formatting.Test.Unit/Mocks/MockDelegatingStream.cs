using System.IO;
using System.Net.Http.Internal;

namespace System.Net.Http.Mocks
{
    internal class MockNonClosingDelegatingStream : NonClosingDelegatingStream
    {
        public MockNonClosingDelegatingStream(Stream innerStream)
            : base(innerStream)
        {
        }
    }
}
