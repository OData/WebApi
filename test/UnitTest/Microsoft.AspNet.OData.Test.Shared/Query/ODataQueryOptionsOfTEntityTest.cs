//-----------------------------------------------------------------------------
// <copyright file="ODataQueryOptionsOfTEntityTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Models;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class ODataQueryOptionsOfTEntityTest
    {
        [Fact]
        public void Ctor_Throws_Argument_IfContextIsofDifferentEntityType()
        {
            var request = RequestFactory.Create();

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            ExceptionAssert.ThrowsArgument(
                () => new ODataQueryOptions<int>(context, request),
                "context", "The entity type 'Microsoft.AspNet.OData.Test.Common.Models.Customer' does not match the expected entity type 'System.Int32' as set on the query context.");
        }

        [Fact]
        public void Ctor_Throws_Argument_IfContextIsUnTyped()
        {
            var request = RequestFactory.Create();

            IEdmModel model = EdmCoreModel.Instance;
            IEdmType elementType = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32);
            ODataQueryContext context = new ODataQueryContext(model, elementType);

            ExceptionAssert.ThrowsArgument(
                () => new ODataQueryOptions<int>(context, request),
                "context", "The property 'ElementClrType' of ODataQueryContext cannot be null.");
        }

        [Fact]
        public void Ctor_SuccedsIfEntityTypesMatch()
        {
            var request = RequestFactory.Create(HttpMethod.Get, "http://server/?$top=10");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, request);
            Assert.Equal("10", query.Top.RawValue);
        }

        [Theory]
        [InlineData("IfMatch")]
        [InlineData("IfNoneMatch")]
        public void GetIfMatchOrNoneMatch_ReturnsETag_SetETagHeaderValue(string header)
        {
            // Arrange
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            customer.HasKey(c => c.Id);
            customer.Property(c => c.Id);
            customer.Property(c => c.Name).IsConcurrencyToken();
            builder.EntitySet<Customer>("Customers");
            IEdmModel model = builder.GetEdmModel();

            var customers = model.FindDeclaredEntitySet("Customers");

            var request = RequestFactory.CreateFromModel(model);

            EntitySetSegment entitySetSegment = new EntitySetSegment(customers);
            ODataPath odataPath = new ODataPath(new[] { entitySetSegment });
            request.ODataContext().Path = odataPath;

            Dictionary<string, object> properties = new Dictionary<string, object> { { "Name", "Foo" } };
            EntityTagHeaderValue etagHeaderValue = new DefaultODataETagHandler().CreateETag(properties);
            if (header.Equals("IfMatch"))
            {
                request.Headers.AddIfMatch(etagHeaderValue);
            }
            else
            {
                request.Headers.AddIfNoneMatch(etagHeaderValue);
            }

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
            var request = RequestFactory.Create();
            ODataModelBuilder builder = new ODataModelBuilder();
            EntityTypeConfiguration<Customer> customer = builder.EntityType<Customer>();
            customer.HasKey(c => c.Id);
            customer.Property(c => c.Id);
            IEdmModel model = builder.GetEdmModel();
            ODataQueryContext context = new ODataQueryContext(model, typeof(Customer));

            // Act
            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, request);
            ETag result = header.Equals("IfMatch") ? query.IfMatch : query.IfNoneMatch;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ApplyTo_ThrowsArgument_If_QueryTypeDoesnotMatch()
        {
            var request = RequestFactory.Create(HttpMethod.Get, "http://server/?$top=10");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, request);

            ExceptionAssert.ThrowsArgument(
                () => query.ApplyTo(Enumerable.Empty<int>().AsQueryable()),
                "query",
                "Cannot apply ODataQueryOptions of 'Microsoft.AspNet.OData.Test.Common.Models.Customer' to IQueryable of 'System.Int32'.");
        }

        [Fact]
        public void ApplyTo_Succeeds_If_QueryTypeDerivesFromOptionsType()
        {
            var request = RequestFactory.Create(HttpMethod.Get, "http://server/?$top=10");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, request);

            ExceptionAssert.DoesNotThrow(
                () => query.ApplyTo(Enumerable.Empty<KirklandCustomer>().AsQueryable()));
        }

        [Fact]
        public void ApplyTo_Succeeds_If_QueryTypeMatchesOptionsType()
        {
            var request = RequestFactory.Create(HttpMethod.Get, "http://server/?$top=10");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, request);

            ExceptionAssert.DoesNotThrow(
                () => query.ApplyTo(Enumerable.Empty<Customer>().AsQueryable()));
        }

        [Fact]
        public void ApplyTo_WithQuerySettings_ThrowsArgument_If_QueryTypeDoesnotMatch()
        {
            var request = RequestFactory.Create(HttpMethod.Get, "http://server/?$top=10");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, request);

            ExceptionAssert.ThrowsArgument(
                () => query.ApplyTo(Enumerable.Empty<int>().AsQueryable(), new ODataQuerySettings()),
                "query",
                "Cannot apply ODataQueryOptions of 'Microsoft.AspNet.OData.Test.Common.Models.Customer' to IQueryable of 'System.Int32'.");
        }

        [Fact]
        public void ApplyTo_WithQuerySettings_Succeeds_If_QueryTypeDerivesFromOptionsType()
        {
            var request = RequestFactory.Create(HttpMethod.Get, "http://server/?$top=10");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, request);

            ExceptionAssert.DoesNotThrow(
                () => query.ApplyTo(Enumerable.Empty<KirklandCustomer>().AsQueryable(), new ODataQuerySettings()));
        }

        [Fact]
        public void ApplyTo_WithQuerySettings_Succeeds_If_QueryTypeMatchesOptionsType()
        {
            var request = RequestFactory.Create(HttpMethod.Get, "http://server/?$top=10");

            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");

            ODataQueryContext context = new ODataQueryContext(builder.GetEdmModel(), typeof(Customer));

            ODataQueryOptions<Customer> query = new ODataQueryOptions<Customer>(context, request);

            ExceptionAssert.DoesNotThrow(
                () => query.ApplyTo(Enumerable.Empty<Customer>().AsQueryable(), new ODataQuerySettings()));
        }
    }
}
