//-----------------------------------------------------------------------------
// <copyright file="CastControllers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.Cast
{
    public class ProductsController : TestODataController
    {
        [EnableQuery]
        public ITestActionResult Get()
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
        public ITestActionResult GetDimensionInCentimeter(int key)
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
    }

}
