//-----------------------------------------------------------------------------
// <copyright file="HttpRequestMessageExtensionsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Hosting;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test
{
    public class HttpRequestMessageExtensionsTest
    {
        [Theory]
        [InlineData(IncludeErrorDetailPolicy.Default, null, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, null, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Default, null, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, true, null, true)]
        [InlineData(IncludeErrorDetailPolicy.Default, true, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Default, true, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, false, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, false, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Default, false, false, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, null, null, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, true, false, true)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, false, true, false)]
        [InlineData(IncludeErrorDetailPolicy.Always, null, null, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, true, false, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, false, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Never, null, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, true, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, false, true, false)]
        public void CreateErrorResponse_Respects_IncludeErrorDetail(IncludeErrorDetailPolicy errorDetail, bool? isLocal, bool? includeErrorDetail, bool detailIncluded)
        {
            HttpConfiguration config = new HttpConfiguration() { IncludeErrorDetailPolicy = errorDetail };
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetConfiguration(config);
            if (isLocal.HasValue)
            {
                request.Properties.Add(HttpPropertyKeys.IsLocalKey, new Lazy<bool>(() => isLocal.Value));
            }
            if (includeErrorDetail.HasValue)
            {
                request.Properties.Add(HttpPropertyKeys.IncludeErrorDetailKey, new Lazy<bool>(() => includeErrorDetail.Value));
            }
            ODataError error = new ODataError()
            {
                ErrorCode = "36",
                Message = "Bad stuff",
                InnerError = new ODataInnerError()
                {
                    Message = "Exception message"
                }
            };

            HttpResponseMessage response = request.CreateErrorResponse(HttpStatusCode.BadRequest, error);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            ODataError contentError;
            Assert.True(response.TryGetContentValue<ODataError>(out contentError));
            Assert.Equal("36", contentError.ErrorCode);
            Assert.Equal("Bad stuff", contentError.Message);
            if (detailIncluded)
            {
                Assert.NotNull(contentError.InnerError);
                Assert.Equal("Exception message", contentError.InnerError.Message);
            }
            else
            {
                Assert.Null(contentError.InnerError);
            }
        }

        [Fact]
        public void ODataProperties_ThrowsArgumentNull_RequestNull()
        {
            HttpRequestMessage request = null;
            ExceptionAssert.ThrowsArgumentNull(
                () => request.ODataProperties(),
                "request");
        }

        [Fact]
        public void SelectExpandClauseSetter_ThrowsArgumentNull()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            ExceptionAssert.ThrowsArgumentNull(
                () => request.ODataProperties().SelectExpandClause = null,
                "value");
        }

        [Fact]
        public void SelectExpandClauseGetter_ReturnsNullByDefault()
        {
            HttpRequestMessage request = new HttpRequestMessage();

            Assert.Null(request.ODataProperties().SelectExpandClause);
        }

        [Fact]
        public void SelectExpandClauseGetter_Returns_SelectExpandClauseSetter()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            SelectExpandClause selectExpandClause = GetMockSelectExpandClause();

            // Act
            request.ODataProperties().SelectExpandClause = selectExpandClause;
            var result = request.ODataProperties().SelectExpandClause;

            // Assert
            Assert.Same(selectExpandClause, result);
        }

        [Fact]
        public void RoutingConventionsStoreGetter_ReturnsEmptyNonNullDictionary()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            IDictionary<string, object> result = request.ODataProperties().RoutingConventionsStore;

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void RoutingConventionsStoreGetter_ReturnsSameInstance_IfCalledMultipleTimes()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            IDictionary<string, object> instance1 = request.ODataProperties().RoutingConventionsStore;
            IDictionary<string, object> instance2 = request.ODataProperties().RoutingConventionsStore;

            // Assert
            Assert.NotNull(instance1);
            Assert.NotNull(instance2);
            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void GetETag_ThrowsArgumentNull_Request()
        {
            // Arrange
            HttpRequestMessage request = null;
            EntityTagHeaderValue headerValue = new EntityTagHeaderValue("\"any\"");

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => request.GetETag(headerValue), "request");
        }

        [Fact]
        public void GetETag_ThrowsInvalidOperation_EmptyRequest()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            EntityTagHeaderValue headerValue = new EntityTagHeaderValue("\"any\"");

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => request.GetETag(headerValue),
                "Request message does not contain an HttpConfiguration object.");
        }

        [Fact]
        public void GetETag_Returns_ETagAny()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var etagHeaderValue = EntityTagHeaderValue.Any;

            // Act
            var result = request.GetETag(etagHeaderValue);

            // Assert
            Assert.True(result.IsAny);
        }

        [Fact]
        public void GetETagTEntity_Returns_ETagAny()
        {
            // Arrange
            var request = new HttpRequestMessage();
            var etagHeaderValue = EntityTagHeaderValue.Any;

            // Act
            var result = request.GetETag<Customer>(etagHeaderValue);

            // Assert
            Assert.True(result.IsAny);
        }

        [Fact]
        public void GetETag_Returns_ETagInHeader()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport(model.Model);

            Dictionary<string, object> properties = new Dictionary<string, object> { { "City", "Foo" } };
            EntityTagHeaderValue etagHeaderValue = new DefaultODataETagHandler().CreateETag(properties);

            ODataPath odataPath = new ODataPath(new EntitySetSegment(model.Customers));
            request.ODataProperties().Path = odataPath;

            // Act
            ETag result = request.GetETag(etagHeaderValue);
            dynamic dynamicResult = result;

            // Assert
            Assert.Equal("Foo", result["City"]);
            Assert.Equal("Foo", dynamicResult.City);
        }

        [Fact]
        public void GetETagTEntity_Returns_ETagInHeader()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport(model.Model);

            Dictionary<string, object> properties = new Dictionary<string, object> { { "City", "Foo" } };
            EntityTagHeaderValue etagHeaderValue = new DefaultODataETagHandler().CreateETag(properties);

            ODataPath odataPath = new ODataPath(new EntitySetSegment(model.Customers));
            request.ODataProperties().Path = odataPath;

            // Act
            ETag<Customer> result = request.GetETag<Customer>(etagHeaderValue);
            dynamic dynamicResult = result;

            // Assert
            Assert.Equal("Foo", result["City"]);
            Assert.Equal("Foo", dynamicResult.City);
        }

        private SelectExpandClause GetMockSelectExpandClause()
        {
            return new SelectExpandClause(new SelectItem[0], allSelected: true);
        }

        [Theory]
        [InlineData("http://localhost/Customers", 10, "http://localhost/Customers?$skip=10")]
        [InlineData("http://localhost/Customers?$filter=Age ge 18", 10, "http://localhost/Customers?$filter=Age%20ge%2018&$skip=10")]
        [InlineData("http://localhost/Customers?$top=20", 10, "http://localhost/Customers?$top=10&$skip=10")]
        [InlineData("http://localhost/Customers?$skip=5&$top=10", 2, "http://localhost/Customers?$top=8&$skip=7")]
        [InlineData("http://localhost/Customers?$filter=Age ge 18&$orderby=Name&$top=11&$skip=6", 10, "http://localhost/Customers?$filter=Age%20ge%2018&$orderby=Name&$top=1&$skip=16")]
        [InlineData("http://localhost/Customers?testkey%23%2B%3D%3F%26=testvalue%23%2B%3D%3F%26", 10, "http://localhost/Customers?testkey%23%2B%3D%3F%26=testvalue%23%2B%3D%3F%26&$skip=10")]
        public void GetNextPageLink_GetsNextPageLink(string requestUri, int pageSize, string nextPageUri)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            Uri nextPageLink = request.GetNextPageLink(pageSize);

            // Assert
            Assert.Equal(nextPageUri, nextPageLink.AbsoluteUri);
        }

        [Fact]
        public void GetNextPageLink_ThatTakesUri_GetsNextPageLink()
        {
            Uri nextPageLink = Microsoft.AspNet.OData.GetNextPageHelper.GetNextPageLink(new Uri("http://localhost/Customers?$filter=Age ge 18"), 10);
            Assert.Equal("http://localhost/Customers?$filter=Age%20ge%2018&$skip=10", nextPageLink.AbsoluteUri);
        }

        [Fact]
        public void GetNextPageLink_WithNullRequestOrUri_Throws()
        {
            HttpRequestMessage nullRequest = null;
            ExceptionAssert.Throws<ArgumentNullException>(() => { Microsoft.AspNet.OData.Extensions.HttpRequestMessageExtensions.GetNextPageLink(nullRequest, 10); });

            HttpRequestMessage requestWithNullUri = new HttpRequestMessage() { RequestUri = null };
            ExceptionAssert.Throws<ArgumentNullException>(() => { Microsoft.AspNet.OData.Extensions.HttpRequestMessageExtensions.GetNextPageLink(requestWithNullUri, 10); });
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(0.0, true)]
        [InlineData(10.0, true)]
        [InlineData(-9999.0, true)]
        [InlineData(1.0, true)]
        [InlineData(-10.0, true)]
        [InlineData(0.1, false)]
        [InlineData(-1.9, false)]
        [InlineData(0.123456, false)]
        public void GetETag_Returns_ETagInHeader_ForDouble(double value, bool isEqual)
        {
            // Arrange
            Dictionary<string, object> properties = new Dictionary<string, object> { { "Version", value } };
            EntityTagHeaderValue etagHeaderValue = new DefaultODataETagHandler().CreateETag(properties);

            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<MyEtagCustomer>("Customers");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet customers = model.FindDeclaredEntitySet("Customers");
            ODataPath odataPath = new ODataPath(new EntitySetSegment(customers));

            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport(model);
            request.ODataProperties().Path = odataPath;

            // Act
            ETag result = request.GetETag(etagHeaderValue);
            dynamic dynamicResult = result;

            // Assert
            double actual = Assert.IsType<double>(result["Version"]);
            Assert.Equal(actual, dynamicResult.Version);

            if (isEqual)
            {
                Assert.Equal(value, actual);
            }
            else
            {
                Assert.NotEqual(value, actual);

                Assert.True(actual - value < 0.0000001);
            }
        }

        public class MyEtagCustomer
        {
            public int Id { get; set; }

            [ConcurrencyCheck]
            public double Version { get; set; }
        }

        [Theory]
        [InlineData((byte)1, (short)2, 3)]
        [InlineData(Byte.MaxValue, Int16.MaxValue, Int64.MaxValue)]
        [InlineData(Byte.MinValue, Int16.MinValue, Int64.MinValue)]
        public void GetETag_Returns_ETagInHeader_ForInteger(byte byteValue, short shortValue, long longValue)
        {
            // Arrange
            Dictionary<string, object> properties = new Dictionary<string, object>
            {
                { "ByteVal", byteValue },
                { "LongVal", longValue },
                { "ShortVal", shortValue }
            };
            EntityTagHeaderValue etagHeaderValue = new DefaultODataETagHandler().CreateETag(properties);

            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<MyEtagOrder>("Orders");
            IEdmModel model = builder.GetEdmModel();
            IEdmEntitySet orders = model.FindDeclaredEntitySet("Orders");
            ODataPath odataPath = new ODataPath(new EntitySetSegment(orders));
            HttpRequestMessage request = new HttpRequestMessage();
            request.EnableHttpDependencyInjectionSupport(model);
            request.ODataProperties().Path = odataPath;

            // Act
            ETag result = request.GetETag(etagHeaderValue);
            dynamic dynamicResult = result;

            // Assert
            byte actualByte = Assert.IsType<byte>(result["ByteVal"]);
            Assert.Equal(actualByte, dynamicResult.ByteVal);
            Assert.Equal(byteValue, actualByte);

            short actualShort = Assert.IsType<short>(result["ShortVal"]);
            Assert.Equal(actualShort, dynamicResult.ShortVal);
            Assert.Equal(shortValue, actualShort);

            long actualLong = Assert.IsType<long>(result["LongVal"]);
            Assert.Equal(actualLong, dynamicResult.LongVal);
            Assert.Equal(longValue, actualLong);

        }

        public class MyEtagOrder
        {
            public int Id { get; set; }

            [ConcurrencyCheck]
            public byte ByteVal { get; set; }

            [ConcurrencyCheck]
            public short ShortVal { get; set; }

            [ConcurrencyCheck]
            public long LongVal { get; set; }
        }

        [Fact]
        public void RequestContainer_Throws_WhenRouteNameIsNotSet()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.SetConfiguration(new HttpConfiguration());

            // Act
            Action action = () => request.GetRequestContainer();

            // Assert
            ExceptionAssert.Throws<InvalidOperationException>(action);
        }
    }
}
#endif
