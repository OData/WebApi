//-----------------------------------------------------------------------------
// <copyright file="UntypedDeltaSerializationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.Untyped
{
    public class UntypedDeltaSerializationTests : WebHostTestBase
    {
        private readonly ITestOutputHelper output;

        public UntypedDeltaSerializationTests(WebHostTestFixture fixture, ITestOutputHelper output)
            : base(fixture)
        {
            this.output = output;
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.MapODataServiceRoute("untyped", "untyped", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            var customers = builder.EntitySet<UntypedCustomer>("UntypedDeltaCustomers");
            customers.EntityType.Property(c => c.Name).IsRequired();
            var orders = builder.EntitySet<UntypedOrder>("UntypedDeltaOrders");
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=full")]
        public async Task UntypedDeltaWorksInAllFormats(string acceptHeader)
        {
            string url = "/untyped/UntypedDeltaCustomers?$deltatoken=abc";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + url);

            // by default, the odata version is 4.0. It will throw:
            // "Cannot transition from state 'DeletedResource' to state 'NestedResourceInfo' when writing an OData 4.0 payload.
            // To write content to a deleted resource, please specify ODataVersion 4.01 or greater in MessageWriterSettings."
            request.Headers.Add("OData-Version", "4.01");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response.Content);
            JObject returnedObject = await response.Content.ReadAsObject<JObject>();
            Assert.True(((dynamic)returnedObject).value.Count == 15);

            //Verification of content to validate Payload
            for (int i = 0; i < 10; i++)
            {
                string name = string.Format("Name {0}", i);
                Assert.True(name.Equals(((dynamic)returnedObject).value[i]["Name"].Value));
            }

            for (int i = 10; i < 15; i++)
            {
                Assert.True(i.ToString().Equals(((dynamic)returnedObject).value[i]["@id"].Value));
            }
        }
    }

    public class UntypedDeltaCustomersController : TestODataController
    {
        public IEdmEntityType DeltaCustomerType
        {
            get
            {
                return Request.GetModel().FindType("Microsoft.Test.E2E.AspNet.OData.Formatter.Untyped.UntypedCustomer") as IEdmEntityType;
            }
        }

        public IEdmEntityType DeltaOrderType
        {
            get
            {
                return Request.GetModel().FindType("Microsoft.Test.E2E.AspNet.OData.Formatter.Untyped.UntypedOrder") as IEdmEntityType;
            }
        }

        public IEdmComplexType DeltaAddressType
        {
            get
            {
                return Request.GetModel().FindType("Microsoft.Test.E2E.AspNet.OData.Formatter.Untyped.UntypedAddress") as IEdmComplexType;
            }
        }

        public ITestActionResult Get()
        {
            EdmChangedObjectCollection changedCollection = new EdmChangedObjectCollection(DeltaCustomerType);
            //Changed or Modified objects are represented as EdmDeltaEntityObjects
            for (int i = 0; i < 10; i++)
            {
                dynamic untypedCustomer = new EdmDeltaEntityObject(DeltaCustomerType);
                untypedCustomer.Id = i;
                untypedCustomer.Name = string.Format("Name {0}", i);
                untypedCustomer.FavoriteNumbers = Enumerable.Range(0, i).ToArray();
                changedCollection.Add(untypedCustomer);
            }

            //Deleted objects are represented as EdmDeltaDeletedObjects
            for (int i = 10; i < 15; i++)
            {
                dynamic untypedCustomer = new EdmDeltaDeletedEntityObject(DeltaCustomerType);
                untypedCustomer.Id = i.ToString();
                untypedCustomer.Reason = DeltaDeletedEntryReason.Deleted;
                changedCollection.Add(untypedCustomer);
            }

            return Ok(changedCollection);
        }
    }
}
