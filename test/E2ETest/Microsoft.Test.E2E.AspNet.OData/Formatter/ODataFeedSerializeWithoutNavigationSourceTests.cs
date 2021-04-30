// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    public class ODataFeedSerializeWithoutNavigationSourceTests : WebHostTestBase
    {
        public ODataFeedSerializeWithoutNavigationSourceTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration config)
        {
            var controllers = new[] { typeof(AnyController), typeof(MetadataController) };
            config.AddControllers(controllers);

            config.MapODataServiceRoute("odata", "odata", GetModel(config));
        }

        private static IEdmModel GetModel(WebRouteConfiguration config)
        {
            ODataModelBuilder builder = config.CreateConventionModelBuilder();
            builder.EntitySet<DerivedTypeA>("SetA");
            builder.EntitySet<DerivedTypeB>("SetB");

            builder.EntityType<BaseType>(); // this line is necessary.
            builder.Function("ReturnAll").ReturnsCollection<BaseType>();

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task CanSerializeFeedWithoutNavigationSource()
        {
            // Arrange
            string requestUri = BaseAddress + "/odata/ReturnAll";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Act
            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            JObject content = await response.Content.ReadAsObject<JObject>();
            Assert.Contains("/odata/$metadata#Collection(Microsoft.Test.E2E.AspNet.OData.Formatter.BaseType)", content["@odata.context"].ToString());

            Assert.Equal(2, content["value"].Count());

            // #1
            Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Formatter.DerivedTypeA", content["value"][0]["@odata.type"].ToString());
            Assert.Equal(1, content["value"][0]["Id"]);

            // #2
            Assert.Equal("#Microsoft.Test.E2E.AspNet.OData.Formatter.DerivedTypeB", content["value"][1]["@odata.type"].ToString());
            Assert.Equal(2, content["value"][1]["Id"]);
        }
    }

    public class AnyController : TestODataController
    {
        public static IList<BaseType> Entities = new List<BaseType>();

        static AnyController()
        {
            DerivedTypeA a = new DerivedTypeA
            {
                Id = 1,
                Name = "Name #1",
                PropertyA = 88
            };
            Entities.Add(a);

            DerivedTypeB b = new DerivedTypeB
            {
                Id = 2,
                Name = "Name #2",
                PropertyB = 99.9,
            };
            Entities.Add(b);
        }

        [HttpGet]
        [ODataRoute("ReturnAll")]
        public ITestActionResult ReturnAll()
        {
            return Ok(Entities);
        }
    }

    public abstract class BaseType
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class DerivedTypeA : BaseType
    {
        public int PropertyA { get; set; }
    }

    public class DerivedTypeB : BaseType
    {
        public double PropertyB { get; set; }
    }
}
