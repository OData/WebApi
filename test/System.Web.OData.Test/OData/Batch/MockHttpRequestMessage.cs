// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;

namespace System.Web.Http
{
    internal class MockHttpRequestMessage : HttpRequestMessage
    {
        public bool IsDisposed { get; set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
        }
    }
}