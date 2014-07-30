// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.TestCommon.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;
using ODataPath = System.Web.OData.Routing.ODataPath;
using ODataPathSegment = System.Web.OData.Routing.ODataPathSegment;

namespace System.Web.OData.Query
{
    public class ODataQueryOptionsOfTEntityTest
    {
        [Fact]
        public void Ctor_Throws_Argument_IfContextIsofDifferentEntityType()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            Assert.ThrowsArgument(
                () => new ODataQueryOptions<int>(context, new HttpRequestMessage()),
                "context", "The entity type 'System.Web.OData.TestCommon.Models.Customer' does not match the expected entity type 'System.Int32' as set on the query context.");
        }

        [Fact]
        public void Ctor_Throws_Argument_IfContextIsUnTyped()
        {
            IEdmModel model = EdmCoreModel.Instance;
            IEdmType elementType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32);
            ODataQueryContext context = new ODataQueryContext(model, elementType);

            Assert.ThrowsArgument(
                () => new ODataQueryOptions<int>(context, new HttpRequestMessage()),
                "context", "The property 'ElementClrType' of ODataQueryContext cannot be null.");
        }

        [Fact]
        public void Ctor_SuccedsIfEntityTypesMatch()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, new HttpRequestMessage(HttpMethod.Get, "http://server/?$top=10"));
            Assert.Equal("10", query.Top.RawValue);
        }

        [Theory]
        [InlineData("IfMatch")]
        [InlineData("IfNoneMatch")]
        public void GetIfMatchOrNoneMatch_ReturnsETag_SetETagHeaderValue(string header)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            HttpConfiguration cofiguration = new HttpConfiguration();
            request.SetConfiguration(cofiguration);
            Dictionary<string, object> properties = new Dictionary<string, object> { { "Name", "Foo" } };
            EntityTagHeaderValue etagHeaderValue = new DefaultODataETagHandler().CreateETag(properties);
            if (header.Equals("IfMatch"))
            {
                request.Headers.IfMatch.Add(etagHeaderValue);
            }
            else
            {
                request.Headers.IfNoneMatch.Add(etagHeaderValue);
            }

            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            customer.HasKey(c => c.Id);
            customer.Property(c => c.Id);
            customer.Property(c => c.Name).IsConcurrencyToken();
            IEdmModel model = builder.GetEdmModel();

            Mock<ODataPathSegment> mockSegment = new Mock<ODataPathSegment> { CallBase = true };
            mockSegment.Setup(s => s.GetEdmType(null)).Returns(model.GetEdmType(typeof(Customer)));
            mockSegment.Setup(s => s.GetNavigationSource(null)).Returns((IEdmNavigationSource)null);
            ODataPath odataPath = new ODataPath(new[] { mockSegment.Object });
            request.ODataProperties().Path = odataPath;
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer));

            // Act
            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, request);
            ETag result = header.Equals("IfMatch") ? query.IfMatch : query.IfNoneMatch;
            dynamic dynamicResult = result;

            // Assert
            Assert.Equal("Foo", result["Name"]);
            Assert.Equal("Foo", dynamicResult.Name);
        }

        [Theory]
        [InlineData("IfMatch")]
        [InlineData("IfNoneMatch")]
        public void GetIfMatchOrNoneMatch_ETagIsNull_IfETagHeaderValueNotSet(string header)
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            customer.HasKey(c => c.Id);
            customer.Property(c => c.Id);
            IEdmModel model = builder.GetEdmModel();
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer));

            // Act
            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, new HttpRequestMessage());
            ETag result = header.Equals("IfMatch") ? query.IfMatch : query.IfNoneMatch;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ApplyTo_ThrowsArgument_If_QueryTypeDoesnotMatch()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, new HttpRequestMessage(HttpMethod.Get, "http://server/?$top=10"));

            Assert.ThrowsArgument(
                () => query.ApplyTo(Enumerable.Empty<int>().AsQueryable()),
                "query",
                "Cannot apply ODataQueryOptions of 'System.Web.OData.TestCommon.Models.Customer' to IQueryable of 'System.Int32'.");
        }

        [Fact]
        public void ApplyTo_Succeeds_If_QueryTypeDerivesFromOptionsType()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, new HttpRequestMessage(HttpMethod.Get, "http://server/?$top=10"));

            Assert.DoesNotThrow(
                () => query.ApplyTo(Enumerable.Empty<KirklandCustomer>().AsQueryable()));
        }

        [Fact]
        public void ApplyTo_Succeeds_If_QueryTypeMatchesOptionsType()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, new HttpRequestMessage(HttpMethod.Get, "http://server/?$top=10"));

            Assert.DoesNotThrow(
                () => query.ApplyTo(Enumerable.Empty<Customer>().AsQueryable()));
        }

        [Fact]
        public void ApplyTo_WithQuerySettings_ThrowsArgument_If_QueryTypeDoesnotMatch()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, new HttpRequestMessage(HttpMethod.Get, "http://server/?$top=10"));

            Assert.ThrowsArgument(
                () => query.ApplyTo(Enumerable.Empty<int>().AsQueryable(), new ODataQuerySettings()),
                "query",
                "Cannot apply ODataQueryOptions of 'System.Web.OData.TestCommon.Models.Customer' to IQueryable of 'System.Int32'.");
        }

        [Fact]
        public void ApplyTo_WithQuerySettings_Succeeds_If_QueryTypeDerivesFromOptionsType()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, new HttpRequestMessage(HttpMethod.Get, "http://server/?$top=10"));

            Assert.DoesNotThrow(
                () => query.ApplyTo(Enumerable.Empty<KirklandCustomer>().AsQueryable(), new ODataQuerySettings()));
        }

        [Fact]
        public void ApplyTo_WithQuerySettings_Succeeds_If_QueryTypeMatchesOptionsType()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, new HttpRequestMessage(HttpMethod.Get, "http://server/?$top=10"));

            Assert.DoesNotThrow(
                () => query.ApplyTo(Enumerable.Empty<Customer>().AsQueryable(), new ODataQuerySettings()));
        }
    }
}
