// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace Microsoft.Test.E2E.AspNet.OData.Cast
{
    public class ProductsController : ODataController
    {
        [EnableQuery]
        public IHttpActionResult Get()
        {
            if (GetRoutePrefix() == "EF")
            {
                return Ok(DataSource.EfProducts);
            }
            else
            {
                return Ok(DataSource.InMemoryProducts);
            }
        }

        [EnableQuery]
        public IHttpActionResult GetDimensionInCentimeter(int key)
        {
            if (GetRoutePrefix() == "EF")
            {
                Product product = DataSource.EfProducts.Single(p => p.ID == key);
                return Ok(product.DimensionInCentimeter);
            }
            else
            {
                Product product = DataSource.InMemoryProducts.Single(p => p.ID == key);
                return Ok(product.DimensionInCentimeter);
            }
        }

        private string GetRoutePrefix()
        {
            ODataRoute oDataRoute = Request.GetRouteData().Route as ODataRoute;
            return oDataRoute.RoutePrefix;
        }
    }

}
