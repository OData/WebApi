// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;

namespace System.Web.OData.Test
{
    internal class MockHttpResponseMessage : HttpResponseMessage
    {
        public bool IsDisposed { get; set; }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
        }
    }
}