//-----------------------------------------------------------------------------
// <copyright file="MockHttpResponseMessage.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;

namespace Microsoft.AspNet.OData.Test.Batch
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
