// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    [EntitySetAttribute("SpecialCharactersLinkGenerationTests")]
    [Key("Name")]
    public class SpecialCharactersLinkGenerationTestsModel
    {
        [System.ComponentModel.DataAnnotations.Key]
        public string Name { get; set; }
    }

    public class SpecialCharactersLinkGenerationTestsController : ODataController
    {
        static SpecialCharactersLinkGenerationTestsController()
        {
            var todoes = new List<SpecialCharactersLinkGenerationTestsModel>();
            foreach (var c in "$&+,/:;=?@ <>#%{}|\\^~[]` ")
            {
                // Skip: 498
                if ("/ \\+?#=".Contains(c))//TODO: '=' is added when migration from odata v3 to v4. and the originally the test fails.
                {
                    continue;
                }

                todoes.Add(new SpecialCharactersLinkGenerationTestsModel()
                {
                    Name = c.ToString()
                });
            }
            Todoes = todoes;
        }

        public static IEnumerable<SpecialCharactersLinkGenerationTestsModel> Todoes { get; set; }

        public IQueryable<SpecialCharactersLinkGenerationTestsModel> Get()
        {
            return Todoes.AsQueryable();
        }

        protected SpecialCharactersLinkGenerationTestsModel GetEntityByKey(string key)
        {
            return Todoes.FirstOrDefault(t => t.Name == key);
        }

        protected SpecialCharactersLinkGenerationTestsModel PatchEntity(string key, Delta<SpecialCharactersLinkGenerationTestsModel> patch)
        {
            var todo = Todoes.FirstOrDefault(t => t.Name == key);
            return todo;
        }
    }

    public class SpecialCharactersLinkGenerationWebTestsController : ODataController
    {
        static SpecialCharactersLinkGenerationWebTestsController()
        {
            var todoes = new List<SpecialCharactersLinkGenerationTestsModel>();
            foreach (var c in "$&+,/:;=?@ <>#%{}|\\^~[]` ")
            {
                // Skip: it blocked by IIS settings
                if ("<>*%:+/\\&:?#=".Contains(c))//TODO: '=' is added when migration from odata v3 to v4. and the originally the test fails.
                {
                    continue;
                }

                todoes.Add(new SpecialCharactersLinkGenerationTestsModel()
                {
                    Name = c.ToString()
                });
            }
            Todoes = todoes;
        }

        public static IEnumerable<SpecialCharactersLinkGenerationTestsModel> Todoes { get; set; }

        public IQueryable<SpecialCharactersLinkGenerationTestsModel> Get()
        {
            return Todoes.AsQueryable();
        }

        protected SpecialCharactersLinkGenerationTestsModel GetEntityByKey(string key)
        {
            return Todoes.FirstOrDefault(t => t.Name == key);
        }

        protected SpecialCharactersLinkGenerationTestsModel PatchEntity(string key, Delta<SpecialCharactersLinkGenerationTestsModel> patch)
        {
            var todo = Todoes.FirstOrDefault(t => t.Name == key);
            return todo;
        }
    }

    public class SpecialCharactersLinkGenerationTests : WebHostTestBase
    {
        public SpecialCharactersLinkGenerationTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.EnableODataSupport(GetEdmModel());
        }

        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<SpecialCharactersLinkGenerationTestsModel>("SpecialCharactersLinkGenerationTests");

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task TestSpecialCharactersInPrimaryKey()
        {
            var context = new DataServiceContext(new Uri(this.BaseAddress));
            context.Format.UseJson(GetEdmModel());

            var query = context.CreateQuery<SpecialCharactersLinkGenerationTestsModel>("SpecialCharactersLinkGenerationTests");
            var todoes = await query.ExecuteAsync();
            bool success = true;

            foreach (var todo in todoes)
            {
                try
                {
                    Uri selfLink;
                    Assert.True(context.TryGetUri(todo, out selfLink));
                    Console.WriteLine(selfLink);

                    await context.ExecuteAsync<SpecialCharactersLinkGenerationTestsModel>(selfLink, "GET", true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    success = false;
                }
            }

            Assert.True(success);
        }
    }

    public class SpecialCharactersLinkGenerationWebTests : WebHostTestBase
    {
        public SpecialCharactersLinkGenerationWebTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel());
        }

        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<SpecialCharactersLinkGenerationTestsModel>("SpecialCharactersLinkGenerationWebTests");
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task TestSpecialCharactersInPrimaryKey()
        {
            var client = new DataServiceContext(new Uri(this.BaseAddress));
            client.Format.UseJson(GetEdmModel());

            var query = client.CreateQuery<SpecialCharactersLinkGenerationTestsModel>("SpecialCharactersLinkGenerationWebTests");
            var todoes = await query.ExecuteAsync();

            bool success = true;
            foreach (var todo in todoes)
            {
                try
                {
                    Uri selfLink;
                    Assert.True(client.TryGetUri(todo, out selfLink));

                    await client.ExecuteAsync<SpecialCharactersLinkGenerationTestsModel>(selfLink, "GET", true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    success = false;
                }
            }

            Assert.True(success);
        }
    }
}
