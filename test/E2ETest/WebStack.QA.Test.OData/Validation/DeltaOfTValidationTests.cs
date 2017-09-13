// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Common.XUnit;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Validation
{
    [NuwaFramework]
    public class DeltaOfTValidationTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "odata", GetModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<PatchCustomer> patchCustomer = builder.EntitySet<PatchCustomer>("PatchCustomers");
            patchCustomer.EntityType.Property(p => p.ExtraProperty).IsRequired();
            return builder.GetEdmModel();
        }


        public static TheoryDataSet<object, HttpStatusCode, string> CanValidatePatchesData
        {
            get
            {
                TheoryDataSet<object, HttpStatusCode, string> data = new TheoryDataSet<object, HttpStatusCode, string>();
                data.Add(new { Id = 5, Name = "Some name", ExtraProperty = "Another value" }, HttpStatusCode.BadRequest, "ExtraProperty : The field ExtraProperty must match the regular expression 'Some value'.\r\n");
                data.Add(new { }, HttpStatusCode.OK, "");
                return data;
            }
        }

        [Theory]
        [PropertyData("CanValidatePatchesData")]
        public void CanValidatePatches(object payload, HttpStatusCode expectedResponseCode, string message)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), BaseAddress + "/odata/PatchCustomers(5)");
            request.Content = new StringContent(JObject.FromObject(payload).ToString());
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.Equal(expectedResponseCode, response.StatusCode);
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                dynamic result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                Assert.Equal(message, result["error"].innererror.message.Value);
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


    public class PatchCustomersController : ODataController
    {
        [AcceptVerbs("PATCH", "MERGE")]
        public IHttpActionResult Patch([FromODataUri] int key, Delta<PatchCustomer> patch)
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
