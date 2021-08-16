//-----------------------------------------------------------------------------
// <copyright file="ODataResultTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    [EntitySet("ODataResult_Model1")]
    [Key("ID")]
    public class ODataResult_Model1
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ICollection<ODataResult_Model2> Model2 { get; set; }
    }

    [EntitySet("ODataResult_Model2")]
    [Key("ID")]
    public class ODataResult_Model2
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class ODataResult_Model1Controller : InMemoryODataController<ODataResult_Model1, int>
    {
        public ODataResult_Model1Controller()
            : base("ID")
        {
        }

        [HttpGet]
        public PageResult<ODataResult_Model2> GetModel2(ODataQueryOptions options, int key, int count)
        {
            var models = new List<ODataResult_Model2>();
            for (int i = 0; i < count; i++)
            {
                models.Add(new ODataResult_Model2
                {
                    ID = i,
                    Name = "Test " + i
                });
            }
            var baseUri = new Uri(this.Url.CreateODataLink());

            IEdmEntitySet entitySet = Request.GetModel().EntityContainer.FindEntitySet("ODataResult_Model2");
            var uri = new Uri(this.Url.CreateODataLink(new EntitySetSegment(entitySet)));
            return new PageResult<ODataResult_Model2>(models, baseUri.MakeRelativeUri(uri), count);
        }
    }

    public class ODataResultTests : WebHostTestBase
    {
        private WebRouteConfiguration _configuration;

        public ODataResultTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            _configuration = configuration;

            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var mb = configuration.CreateConventionModelBuilder();
            mb.EntitySet<ODataResult_Model1>("ODataResult_Model1");
            mb.EntitySet<ODataResult_Model2>("ODataResult_Model2");

            return mb.GetEdmModel();
        }

        [Fact]
        public async Task ODataResultWithZeroResultShouldWork()
        {
            // Arrange
            var ctx = new DataServiceContext(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            ctx.Format.UseJson(GetEdmModel(_configuration));

            ctx.AddObject(
                "ODataResult_Model1",
                new ODataResult_Model1()
                {
                    ID = 1,
                    Name = "ABC"
                });
            await ctx.SaveChangesAsync();

            // Act
            var response = await Client.GetWithAcceptAsync(this.BaseAddress + "/ODataResult_Model1(1)/Model2?count=0", "application/json");
            var responseContentString = await response.Content.ReadAsStringAsync();

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Contains("\"@odata.nextLink\":\"ODataResult_Model2\"", responseContentString);
        }
    }
}
