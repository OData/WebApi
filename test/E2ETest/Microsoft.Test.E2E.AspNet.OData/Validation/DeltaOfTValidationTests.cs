// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Validation
{
    public class DeltaOfTValidationTests : WebHostTestBase<DeltaOfTValidationTests>
    {
        public DeltaOfTValidationTests(WebHostTestFixture<DeltaOfTValidationTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "odata", GetModel(config), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel(WebRouteConfiguration config)
        {
            ODataModelBuilder builder = config.CreateConventionModelBuilder();
            EntitySetConfiguration<PatchCustomer> patchCustomer = builder.EntitySet<PatchCustomer>("PatchCustomers");
            patchCustomer.EntityType.Property(p => p.ExtraProperty).IsRequired();
            return builder.GetEdmModel();
        }


        public static TheoryDataSet<int, string> CanValidatePatchesData
        {
            get
            {
                TheoryDataSet<int, string> data = new TheoryDataSet<int, string>();
                data.Add((int)HttpStatusCode.BadRequest, "The field ExtraProperty must match the regular expression 'Some value'");
                data.Add((int)HttpStatusCode.OK, "");
                return data;
            }
        }

        [Theory]
        [MemberData(nameof(CanValidatePatchesData))]
        public async Task CanValidatePatches(int expectedResponseCode, string message)
        {
            object payload = null;
            switch (expectedResponseCode)
            {
                case (int)HttpStatusCode.BadRequest:
                    payload = new { Id = 5, Name = "Some name", ExtraProperty = "Another value" };
                    break;

                case (int)HttpStatusCode.OK:
                    payload = new { };
                    break;
            }

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), BaseAddress + "/odata/PatchCustomers(5)");
            request.Content = new StringContent(JObject.FromObject(payload).ToString());
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.Equal(expectedResponseCode, (int)response.StatusCode);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var result = await response.Content.ReadAsStringAsync();
                Assert.Contains(message, result);
            }
        }
    }

    public class PatchCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [RegularExpression("Some value")]
        public string ExtraProperty { get; set; }
    }


    public class PatchCustomersController : TestODataController
    {
        [AcceptVerbs("PATCH", "MERGE")]
        public ITestActionResult Patch([FromODataUri] int key, [FromBody] Delta<PatchCustomer> patch)
        {
            PatchCustomer c = new PatchCustomer() { Id = key, ExtraProperty = "Some value" };
            patch.Patch(c);
            Validate(c);

            if (ModelState.IsValid)
            {
                return Ok(c);

            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }
}
