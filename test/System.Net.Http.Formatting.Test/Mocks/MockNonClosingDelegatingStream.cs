// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
