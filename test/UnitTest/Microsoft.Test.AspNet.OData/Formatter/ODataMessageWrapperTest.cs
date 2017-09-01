// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter
{
    public class ODataMessageWrapperTest
    {
        [Fact]
        public void ResolveUrl_ThrowsArgumentNull_PayloadUri()
        {
            var message = new ODataMessageWrapper();
            Assert.ThrowsArgumentNull(
                () => message.ConvertPayloadUri(new Uri("http://localhost"), null),
                "payloadUri");
        }

        [Fact]
        public void ResolveUrl_ReturnsNull_IfNoContentIdInUri()
        {
            var message = new ODataMessageWrapper();

            Uri uri = message.ConvertPayloadUri(new Uri("http://localhost"), new Uri("/values", UriKind.Relative));

            Assert.Null(uri);
        }

        [Fact]
        public void ResolveUrl_ReturnsOriginalUri_IfContentIdCannotBeResolved()
        {
            StringContent content = new StringContent(String.Empty);
            var message = new ODataMessageWrapper(new MemoryStream(), content.Headers);

            Uri uri = message.ConvertPayloadUri(new Uri("http://localhost"), new Uri("$1", UriKind.Relative));

            Assert.Equal("$1", uri.OriginalString);
        }

        [Fact]
        public void ResolveUrl_ResolvesUriWithContentId()
        {
            StringContent content = new StringContent(String.Empty);
            Dictionary<string, string> contentIdMapping = new Dictionary<string, string>
            {
                {"1", "http://localhost/values(1)"},
                {"11", "http://localhost/values(11)"},
            };
            var message = new ODataMessageWrapper(new MemoryStream(), content.Headers, contentIdMapping);

            Uri uri = message.ConvertPayloadUri(new Uri("http://localhost"), new Uri("$1", UriKind.Relative));

            Assert.Equal("http://localhost/values(1)", uri.OriginalString);
        }
    }
}