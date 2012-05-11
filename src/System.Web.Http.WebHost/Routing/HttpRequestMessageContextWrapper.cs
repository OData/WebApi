// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;

namespace System.Web.Http.WebHost.Routing
{
    internal class HttpRequestMessageContextWrapper : HttpContextBase
    {
        private HttpRequestMessageWrapper _httpWrapper;

        public HttpRequestMessageContextWrapper(string virtualPathRoot, HttpRequestMessage httpRequest)
        {
            _httpWrapper = new HttpRequestMessageWrapper(virtualPathRoot, httpRequest);
        }

        public override HttpRequestBase Request
        {
            get { return _httpWrapper; }
        }
    }
}
