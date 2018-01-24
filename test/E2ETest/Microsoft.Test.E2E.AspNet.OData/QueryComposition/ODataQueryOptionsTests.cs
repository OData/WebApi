// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class ODataQueryOptions_Todo
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class CollectionOfString : List<string>
    {
        public CollectionOfString()
        {
        }

        public CollectionOfString(IEnumerable<string> list)
            : base(list)
        {
        }
    }

    public class CollectionOfStringResult
    {
        public CollectionOfString List { get; set; }
        public string NextPage { get; set; }
    }

    public class ODataQueryOptionsController : ApiController
    {
        private static List<ODataQueryOptions_Todo> todoes = new List<ODataQueryOptions_Todo>();
        static ODataQueryOptionsController()
        {
            for (int i = 0; i < 100; i++)
            {
                todoes.Add(new ODataQueryOptions_Todo
                {
                    ID = i,
                    Name = "Test" + i
                });
            }
        }

        [HttpGet]
        public IQueryable<ODataQueryOptions_Todo> OptionsOnIEnumerableT(ODataQueryOptions options)
        {
            return options.ApplyTo(todoes.AsQueryable()) as IQueryable<ODataQueryOptions_Todo>;
        }

        [HttpGet]
        public IQueryable<string> OptionsOnIEnumerableString(ODataQueryOptions<ODataQueryOptions_Todo> options)
        {
            return (options.ApplyTo(todoes.AsQueryable()) as IQueryable<ODataQueryOptions_Todo>).Select(t => t.Name);
        }

        [HttpGet]
        public string OptionsOnString(ODataQueryOptions<ODataQueryOptions_Todo> options)
        {
            return (options.ApplyTo(todoes.AsQueryable()) as IQueryable<ODataQueryOptions_Todo>).Select(t => t.Name).First();
        }

        [HttpGet]
        public int GetTopValue(ODataQueryOptions<ODataQueryOptions_Todo> options)
        {
            return options.Top.Value;
        }

        [HttpGet]
        public HttpResponseMessage OptionsOnHttpResponseMessage(ODataQueryOptions<ODataQueryOptions_Todo> options)
        {
            var t = options.ApplyTo(todoes.AsQueryable()) as IQueryable<ODataQueryOptions_Todo>;
            return this.Request.CreateResponse(System.Net.HttpStatusCode.OK, new { Todoes = t });
        }

        [HttpGet]
        public CollectionOfStringResult OptionsWithString(ODataQueryOptions<string> options)
        {
            CollectionOfString list = new CollectionOfString();
            list.Add("One");
            list.Add("Two");
            list.Add("Three");
            return new CollectionOfStringResult
            {
                NextPage = "Test",
                List = new CollectionOfString(options.ApplyTo(list.AsQueryable()) as IQueryable<string>)
            };
        }
    }

    public class ODataQueryOptionsTests : WebHostTestBase
    {
        public ODataQueryOptionsTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.AddODataQueryFilter();
            configuration.EnableDependencyInjection();
        }

        [Fact]
        public async Task OptionsOnIEnumerableTShouldWork()
        {
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/ODataQueryOptions/OptionsOnIEnumerableT?$filter=ID ge 50");
            var actual = await response.Content.ReadAsAsync<IEnumerable<ODataQueryOptions_Todo>>();
            Assert.Equal(50, actual.Count());
        }

        [Fact]
        public async Task OptionsOnStringShouldWork()
        {
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/ODataQueryOptions/OptionsOnString?$filter=ID ge 50");
            var actual = await response.Content.ReadAsAsync<string>();
            Assert.Equal("Test50", actual);
        }

        [Fact]
        public async Task GetTopValueShouldWork()
        {
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/ODataQueryOptions/GetTopValue?$top=50");
            var actual = await response.Content.ReadAsAsync<int>();
            Assert.Equal(50, actual);
        }

        [Fact]
        public async Task OptionsOnHttpResponseMessageShouldWork()
        {
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/ODataQueryOptions/OptionsOnHttpResponseMessage?$filter=ID ge 50");
            var actual = await response.Content.ReadAsStringAsync();
            Console.WriteLine(actual);
        }

        [Fact]
        public void UnitTestOptionsShouldWork()
        {
            ODataQueryOptionsController controller = new ODataQueryOptionsController();

            ODataQueryContext context = new ODataQueryContext(GetEdmModel(), typeof(ODataQueryOptions_Todo), path: null);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?$orderby=Name desc");
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.EnableDependencyInjection();
            request.SetConfiguration(configuration);
            ODataQueryOptions<ODataQueryOptions_Todo> options = new ODataQueryOptions<ODataQueryOptions_Todo>(context, request);
            var result = controller.OptionsOnString(options);
            Assert.Equal("Test99", result);
        }

        [Fact]
        public void UnitTestOptionsOfStringShouldWork()
        {
            ODataQueryOptionsController controller = new ODataQueryOptionsController();

            ODataQueryContext context = new ODataQueryContext(new ODataConventionModelBuilder().GetEdmModel(), typeof(string), path: null);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?$top=1");
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.EnableDependencyInjection();
            request.SetConfiguration(configuration);
            ODataQueryOptions<string> options = new ODataQueryOptions<string>(context, request);
            var result = controller.OptionsWithString(options);
            Assert.Equal("One", result.List.Single());
        }

        private Microsoft.OData.Edm.IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<ODataQueryOptions_Todo>();
            return builder.GetEdmModel();
        }
    }
}
