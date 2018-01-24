// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    public class ErrorMessagesTests : WebHostTestBase
    {
        public ErrorMessagesTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "odata", GetModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ErrorCustomer> customers = builder.EntitySet<ErrorCustomer>("ErrorCustomers");
            return builder.GetEdmModel();
        }
    }

    public class ErrorCustomersController : ODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get()
        {
            return Ok(Enumerable.Range(0, 1).Select(i => new ErrorCustomer
            {
                Id = i,
                Name = "Name i",
                Numbers = null
            }));
        }
    }

    public class ErrorCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<int> Numbers { get; set; }
    }
}
