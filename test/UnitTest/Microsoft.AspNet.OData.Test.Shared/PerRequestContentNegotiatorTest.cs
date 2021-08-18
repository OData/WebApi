//-----------------------------------------------------------------------------
// <copyright file="PerRequestContentNegotiatorTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.OData;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class PerRequestContentNegotiatorTest
    {
        [Fact]
        public void Negotiate_CallGetPerRequestFormatterInstanceFirst()
        {
            HttpConfiguration config = RoutingConfigurationFactory.CreateWithRootContainer("odata");
            HttpRequestMessage request = RequestFactory.Create(config, "odata");
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
#endif
