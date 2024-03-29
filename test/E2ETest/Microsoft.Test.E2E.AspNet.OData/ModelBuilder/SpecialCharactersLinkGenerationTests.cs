//-----------------------------------------------------------------------------
// <copyright file="SpecialCharactersLinkGenerationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
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

    public class SpecialCharactersLinkGenerationTestsController : TestODataController
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

        public SpecialCharactersLinkGenerationTestsModel Get(string key)
        {
            return Todoes.FirstOrDefault(t => t.Name == key);
        }

        public SpecialCharactersLinkGenerationTestsModel Patch(string key, Delta<SpecialCharactersLinkGenerationTestsModel> patch)
        {
            var todo = Todoes.FirstOrDefault(t => t.Name == key);
            return todo;
        }
    }

    public class SpecialCharactersLinkGenerationWebTestsController : TestODataController
    {
        static SpecialCharactersLinkGenerationWebTestsController()
        {
            var todoes = new List<SpecialCharactersLinkGenerationTestsModel>();
            foreach (var c in "$&+,/:;=?@ <>#%{}|\\^~[]` ")
            {
                // Skip: it's blocked by IIS settings
                // The whitespace (' ') is skipped because it was not supported but the test had bug which
                // erroneously reported a successful response
                // if a single whitespace should be allowed as a key parameter, then support for it should
                // be implemented and it should be omitted from this string of skipped chars
                if (" <>*%:+/\\&:?#=".Contains(c))//TODO: '=' is added when migration from odata v3 to v4. and the originally the test fails.
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

        public SpecialCharactersLinkGenerationTestsModel Get(string key)
        {
            return Todoes.FirstOrDefault(t => t.Name == key);
        }

        public SpecialCharactersLinkGenerationTestsModel Patch(string key, Delta<SpecialCharactersLinkGenerationTestsModel> patch)
        {
            var todo = Todoes.FirstOrDefault(t => t.Name == key);
            return todo;
        }
    }

    public class SpecialCharactersLinkGenerationTests : WebHostTestBase
    {
        private IEdmModel _model;

        public SpecialCharactersLinkGenerationTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            _model = GetEdmModel(configuration);
            configuration.EnableODataSupport(_model);
        }

        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<SpecialCharactersLinkGenerationTestsModel>("SpecialCharactersLinkGenerationTests");

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task TestSpecialCharactersInPrimaryKey()
        {
            var context = new DataServiceContext(new Uri(this.BaseAddress));
            context.Format.UseJson(_model);

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

                    var result = await context.ExecuteAsync<SpecialCharactersLinkGenerationTestsModel>(selfLink, "GET", true);
                    var fetchedTodo = result.FirstOrDefault();
                    Assert.NotNull(fetchedTodo);
                    Assert.Equal(todo.Name, fetchedTodo.Name);
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
        private IEdmModel _model;

        public SpecialCharactersLinkGenerationWebTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            _model = GetEdmModel(configuration);
            configuration.EnableODataSupport(_model);
        }

        public static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<SpecialCharactersLinkGenerationTestsModel>("SpecialCharactersLinkGenerationWebTests");
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task TestSpecialCharactersInPrimaryKey()
        {
            var client = new DataServiceContext(new Uri(this.BaseAddress));
            client.Format.UseJson(_model);

            var query = client.CreateQuery<SpecialCharactersLinkGenerationTestsModel>("SpecialCharactersLinkGenerationWebTests");
            var todoes = await query.ExecuteAsync();

            bool success = true;
            foreach (var todo in todoes)
            {
                try
                {
                    Uri selfLink;
                    Assert.True(client.TryGetUri(todo, out selfLink));

                    var result = await client.ExecuteAsync<SpecialCharactersLinkGenerationTestsModel>(selfLink, "GET", true);
                    var fetchedTodo = result.FirstOrDefault();
                    Assert.NotNull(fetchedTodo);
                    Assert.Equal(todo.Name, fetchedTodo.Name);
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
