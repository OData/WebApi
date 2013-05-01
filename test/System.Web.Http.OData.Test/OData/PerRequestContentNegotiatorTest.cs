// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData
{
    public class PerRequestContentNegotiatorTest
    {
        [Fact]
        public void Negotiate_CallGetPerRequestFormatterInstanceFirst()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            MediaTypeFormatter perRequestFormatter = new ODataMediaTypeFormatter(Enumerable.Empty<ODataPayloadKind>()) { Request = request };
            Mock<MediaTypeFormatter> formatter = new Mock<MediaTypeFormatter>();
            formatter
                .Setup(f => f.GetPerRequestFormatterInstance(typeof(void), request, It.IsAny<MediaTypeHeaderValue>()))
                .Returns(perRequestFormatter);
            Mock<IContentNegotiator> innerContentNegotiator = new Mock<IContentNegotiator>();
            innerContentNegotiator
                .Setup(n => n.Negotiate(typeof(void), request, It.Is<IEnumerable<MediaTypeFormatter>>(f => f.First() == perRequestFormatter)))
                .Returns(new ContentNegotiationResult(perRequestFormatter, MediaTypeHeaderValue.Parse("application/json")));

            IContentNegotiator contentNegotiator = new PerRequestContentNegotiator(innerContentNegotiator.Object);
            var negotiationResult = contentNegotiator.Negotiate(typeof(void), request, new MediaTypeFormatter[] { formatter.Object });

            Assert.Same(perRequestFormatter, negotiationResult.Formatter);
        }
    }
}
