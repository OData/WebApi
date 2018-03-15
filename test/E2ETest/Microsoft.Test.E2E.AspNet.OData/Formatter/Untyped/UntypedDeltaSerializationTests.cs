﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.Untyped
{
    public class UntypedDeltaSerializationTests : WebHostTestBase
    {
        public UntypedDeltaSerializationTests(WebHostTestFixture fixture)
            :base(fixture)
        {
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
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response.Content);
            JObject returnedObject = await response.Content.ReadAsObject<JObject>();
            Assert.True(((dynamic)returnedObject).value.Count == 15);

            //Verification of content to validate Payload
            for (int i = 0 ; i < 10 ; i++)
            {
                string name = string.Format("Name {0}", i);
                Assert.True(name.Equals(((dynamic)returnedObject).value[i]["Name"].Value));
            }

            for (int i=10 ; i < 15 ; i++)
            {
                string contextUrl = BaseAddress.ToLowerInvariant() + "/untyped/$metadata#UntypedDeltaCustomers/$deletedEntity";
                Assert.True(contextUrl.Equals(((dynamic)returnedObject).value[i]["@odata.context"].Value));
                Assert.True(i.ToString().Equals(((dynamic)returnedObject).value[i]["id"].Value));
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
