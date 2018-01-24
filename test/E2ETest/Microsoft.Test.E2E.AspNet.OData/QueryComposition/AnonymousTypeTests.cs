// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class AnonymousType_Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class AnonymousTypeController : ApiController
    {
        public IQueryable Get()
        {
            var personList = new List<AnonymousType_Person> 
            {
                new AnonymousType_Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new AnonymousType_Person { Id = 2, FirstName = "Eugene", LastName = "Agafonov" }
            };
            var query = from p in personList select new { p.FirstName, p.LastName };
            return query.AsQueryable();
        }
    }

    public class AnonymousTypeTests : WebHostTestBase
    {
        public AnonymousTypeTests(WebHostTestFixture fixture)
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
        public async Task ReturnIQueryableOfAnonymousTypeShouldWork()
        {
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/AnonymousType/Get?$filter=FirstName eq 'John'");
            var actual = await response.Content.ReadAsAsync<IEnumerable<AnonymousType_Person>>();
            Assert.Single(actual);
            Assert.Equal("John", actual.First().FirstName);
        }
    }
}
