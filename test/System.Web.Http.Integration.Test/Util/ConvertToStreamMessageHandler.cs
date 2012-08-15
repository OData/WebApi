// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Util
{
    internal class ConvertToStreamMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return
                ToStreamContent(request.Content)
                .Then(content =>
                {
                    request.Content = content;
                    return base.SendAsync(request, cancellationToken);
                })
                .Then(response =>
                {
                    return
                        ToStreamContent(response.Content)
                        .Then(content =>
                        {
                            response.Content = content;
                            return response;
                        });
                });
        }

        private Task<HttpContent> ToStreamContent(HttpContent content)
        {
            ObjectContent objectContent = content as ObjectContent;
            if (objectContent != null)
            {
                return objectContent
                    .ReadAsStreamAsync()
                    .Then<Stream, HttpContent>(stream =>
                    {
                        StreamContent streamContent = new StreamContent(stream);
                        foreach (var header in objectContent.Headers)
                        {
                            streamContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }

                        return streamContent;
                    });
            }
            else
            {
                return TaskHelpers.FromResult(content);
            }
        }
    }
}
