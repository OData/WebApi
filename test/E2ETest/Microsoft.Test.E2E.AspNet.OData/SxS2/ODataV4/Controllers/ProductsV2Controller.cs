//-----------------------------------------------------------------------------
// <copyright file="ProductsV2Controller.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.E2E.AspNet.OData.SxS2.ODataV4.Models;

namespace Microsoft.Test.E2E.AspNet.OData.SxS2.ODataV4.Controllers
{
    [ODataRoutePrefix("Products")]
    public class ProductsV2Controller : ODataController
    {
        private readonly List<Product> _products;
        public ProductsV2Controller()
        {
            _products = new List<Product>();
            _products.AddRange(
                Enumerable.Range(1, 5).Select(n =>
                    new Product()
                    {
                        Id = n,
                        Title = string.Concat("Title", n),
                        ManufactureDateTime = DateTimeOffset.Now,
                    }));
        }

        // GET odata/Products
        [EnableQuery]
        [ODataRoute("")]
        public IQueryable<Product> GetProducts()
        {
            return _products.AsQueryable();
        }

        // GET odata/Products(5)
        [EnableQuery]
        [ODataRoute("({key})")]
        public SingleResult<Product> GetProduct([FromODataUri] int key)
        {
            return SingleResult.Create(_products.Where(product => product.Id == key).AsQueryable());
        }
    }
}
