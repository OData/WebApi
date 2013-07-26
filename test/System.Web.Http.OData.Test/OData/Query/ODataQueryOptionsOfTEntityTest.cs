// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.TestCommon.Models;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query
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
                "context", "The entity type 'System.Web.Http.OData.TestCommon.Models.Customer' does not match the expected entity type 'System.Int32' as set on the query context.");
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
                "Cannot apply ODataQueryOptions of 'System.Web.Http.OData.TestCommon.Models.Customer' to IQueryable of 'System.Int32'.");
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
                "Cannot apply ODataQueryOptions of 'System.Web.Http.OData.TestCommon.Models.Customer' to IQueryable of 'System.Int32'.");
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
