// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Owin
{
    internal static class HttpMessageHandlerExtensions
    {
        public static Task<HttpResponseMessage> SendAsync(this HttpMessageHandler handler, HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpMessageInvoker invoker = new HttpMessageInvoker(handler, disposeHandler: false);
            return invoker.SendAsync(request, cancellationToken);
        }
    }
}
