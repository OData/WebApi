// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class AnonymousType_Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class AnonymousTypeController : TestNonODataController
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

    public class AnonymousTypeTests : WebHostTestBase<AnonymousTypeTests>
    {
        public AnonymousTypeTests(WebHostTestFixture<AnonymousTypeTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.AddODataQueryFilter();
            configuration.EnableDependencyInjection();
        }

        [Fact]
        public async Task ReturnIQueryableOfAnonymousTypeShouldWork()
        {
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/AnonymousType/Get?$filter=FirstName eq 'John'");
            var actual = await response.Content.ReadAsObject<IEnumerable<AnonymousType_Person>>();
            Assert.Single(actual);
            Assert.Equal("John", actual.First().FirstName);
        }
    }
}
