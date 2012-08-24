// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Net.Http.Formatting.Mocks
{
    public class MockAsyncCallback
    {
        public bool WasInvoked { get; private set; }

        public IAsyncResult AsyncResult { get; private set; }

        public void Handler(IAsyncResult result)
        {
            WasInvoked = true;
            AsyncResult = result;
        }
    }
}
