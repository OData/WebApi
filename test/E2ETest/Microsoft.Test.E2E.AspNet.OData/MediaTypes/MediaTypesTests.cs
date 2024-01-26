//-----------------------------------------------------------------------------
// <copyright file="MediaTypesTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.MediaTypes
{
    public class MediaTypesTests : WebHostTestBase
    {
        public MediaTypesTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute(
                "odata",
                "",
                GetEdmModel(configuration),
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();

            builder.EntitySet<MediaTypesOrder>("MediaTypesOrders");

            return builder.GetEdmModel();
        }

        public static IEnumerable<object[]> GetMediaTypeTestData()
        {
            return new List<object[]>
            {
                new object[]
                {
                    "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false",
                    "{{\"@odata.context\":\"{0}/$metadata#MediaTypesOrders/$entity\",\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=true",
                    "{{\"@odata.context\":\"{0}/$metadata#MediaTypesOrders/$entity\",\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=false",
                    "{{\"@odata.context\":\"{0}/$metadata#MediaTypesOrders/$entity\",\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=true",
                    "{{\"@odata.context\":\"{0}/$metadata#MediaTypesOrders/$entity\",\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=minimal;IEEE754Compatible=false",
                    "{{\"@odata.context\":\"{0}/$metadata#MediaTypesOrders/$entity\",\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=minimal;IEEE754Compatible=true",
                    "{{\"@odata.context\":\"{0}/$metadata#MediaTypesOrders/$entity\",\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=false",
                    $"{{{{\"@odata.context\":\"{{0}}/$metadata#MediaTypesOrders/$entity\",\"@odata.type\":\"#{typeof(MediaTypesOrder).FullName}\",\"@odata.id\":\"{{0}}/MediaTypesOrders(1)\",\"@odata.editLink\":\"{{0}}/MediaTypesOrders(1)\",\"Id\":1,\"Amount@odata.type\":\"#Decimal\",\"Amount\":130,\"TrackingNumber@odata.type\":\"#Int64\",\"TrackingNumber\":9223372036854775807}}}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=true",
                    $"{{{{\"@odata.context\":\"{{0}}/$metadata#MediaTypesOrders/$entity\",\"@odata.type\":\"#{typeof(MediaTypesOrder).FullName}\",\"@odata.id\":\"{{0}}/MediaTypesOrders(1)\",\"@odata.editLink\":\"{{0}}/MediaTypesOrders(1)\",\"Id\":1,\"Amount@odata.type\":\"#Decimal\",\"Amount\":\"130\",\"TrackingNumber@odata.type\":\"#Int64\",\"TrackingNumber\":\"9223372036854775807\"}}}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=false",
                    $"{{{{\"@odata.context\":\"{{0}}/$metadata#MediaTypesOrders/$entity\",\"@odata.type\":\"#{typeof(MediaTypesOrder).FullName}\",\"@odata.id\":\"{{0}}/MediaTypesOrders(1)\",\"@odata.editLink\":\"{{0}}/MediaTypesOrders(1)\",\"Id\":1,\"Amount@odata.type\":\"#Decimal\",\"Amount\":130,\"TrackingNumber@odata.type\":\"#Int64\",\"TrackingNumber\":9223372036854775807}}}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=true",
                    $"{{{{\"@odata.context\":\"{{0}}/$metadata#MediaTypesOrders/$entity\",\"@odata.type\":\"#{typeof(MediaTypesOrder).FullName}\",\"@odata.id\":\"{{0}}/MediaTypesOrders(1)\",\"@odata.editLink\":\"{{0}}/MediaTypesOrders(1)\",\"Id\":1,\"Amount@odata.type\":\"#Decimal\",\"Amount\":\"130\",\"TrackingNumber@odata.type\":\"#Int64\",\"TrackingNumber\":\"9223372036854775807\"}}}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=full;IEEE754Compatible=false",
                    $"{{{{\"@odata.context\":\"{{0}}/$metadata#MediaTypesOrders/$entity\",\"@odata.type\":\"#{typeof(MediaTypesOrder).FullName}\",\"@odata.id\":\"{{0}}/MediaTypesOrders(1)\",\"@odata.editLink\":\"{{0}}/MediaTypesOrders(1)\",\"Id\":1,\"Amount@odata.type\":\"#Decimal\",\"Amount\":130,\"TrackingNumber@odata.type\":\"#Int64\",\"TrackingNumber\":9223372036854775807}}}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=full;IEEE754Compatible=true",
                    $"{{{{\"@odata.context\":\"{{0}}/$metadata#MediaTypesOrders/$entity\",\"@odata.type\":\"#{typeof(MediaTypesOrder).FullName}\",\"@odata.id\":\"{{0}}/MediaTypesOrders(1)\",\"@odata.editLink\":\"{{0}}/MediaTypesOrders(1)\",\"Id\":1,\"Amount@odata.type\":\"#Decimal\",\"Amount\":\"130\",\"TrackingNumber@odata.type\":\"#Int64\",\"TrackingNumber\":\"9223372036854775807\"}}}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=false",
                    "{{\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=true",
                    "{{\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=false",
                    "{{\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=true",
                    "{{\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=none;IEEE754Compatible=false",
                    "{{\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}}"
                },
                new object[]
                {
                    "application/json;odata.metadata=none;IEEE754Compatible=true",
                    "{{\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}}"
                },
                new object[]
                {
                    "application/json;odata.streaming=false;IEEE754Compatible=false",
                    "{{\"@odata.context\":\"{0}/$metadata#MediaTypesOrders/$entity\",\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}}"
                },
                new object[]
                {
                    "application/json;odata.streaming=false;IEEE754Compatible=true",
                    "{{\"@odata.context\":\"{0}/$metadata#MediaTypesOrders/$entity\",\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}}"
                },
                new object[]
                {
                    "application/json;odata.streaming=true;IEEE754Compatible=false",
                    "{{\"@odata.context\":\"{0}/$metadata#MediaTypesOrders/$entity\",\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}}"
                },
                new object[]
                {
                    "application/json;odata.streaming=true;IEEE754Compatible=true",
                    "{{\"@odata.context\":\"{0}/$metadata#MediaTypesOrders/$entity\",\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}}"
                },
                new object[]
                {
                    "application/json;IEEE754Compatible=false",
                    "{{\"@odata.context\":\"{0}/$metadata#MediaTypesOrders/$entity\",\"Id\":1,\"Amount\":130,\"TrackingNumber\":9223372036854775807}}"
                },
                new object[]
                {
                    "application/json;IEEE754Compatible=true",
                    "{{\"@odata.context\":\"{0}/$metadata#MediaTypesOrders/$entity\",\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}}"
                },
            };
        }

        [Theory]
        [MemberData(nameof(GetMediaTypeTestData))]
        public async Task VerifyResultForMediaTypeInAcceptHeader(string mediaType, string expected)
        {
            // Arrange
            var baseAddress = this.BaseAddress.ToLowerInvariant();
            var requestUri = $"{baseAddress}/MediaTypesOrders(1)";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(mediaType));

            // Act
            var response = await this.Client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(string.Format(expected, baseAddress), result);
        }

        [Theory]
        [MemberData(nameof(GetMediaTypeTestData))]
        public async Task VerifyResultForMediaTypeInFormatQueryOption(string mediaType, string expected)
        {
            // Arrange
            var baseAddress = this.BaseAddress.ToLowerInvariant();
            var requestUri = $"{baseAddress}/MediaTypesOrders(1)?$format={mediaType}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal(string.Format(expected, baseAddress), result);
        }

        [Theory]
        [InlineData("application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=true")]
        [InlineData("application/json;odata.metadata=minimal;IEEE754Compatible=true;odata.streaming=true")]
        [InlineData("application/json;IEEE754Compatible=true;odata.metadata=minimal;odata.streaming=true")]
        public async Task VerifyPositionOfIEEE754CompatibleParameterInMediaTypeShouldNotMatter(string mediaType)
        {
            // Arrange
            var baseAddress = this.BaseAddress.ToLowerInvariant();
            var requestUri = $"{baseAddress}/MediaTypesOrders(1)";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(mediaType));

            // Act
            var response = await this.Client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();

            Assert.Equal($"{{\"@odata.context\":\"{baseAddress}/$metadata#MediaTypesOrders/$entity\",\"Id\":1,\"Amount\":\"130\",\"TrackingNumber\":\"9223372036854775807\"}}", result);
        }
    }
}
