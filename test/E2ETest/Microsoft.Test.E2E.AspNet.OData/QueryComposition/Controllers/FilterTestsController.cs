//-----------------------------------------------------------------------------
// <copyright file="FilterTestsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Models;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.Products;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition.Controllers
{
    public class FilterTestsController : TestNonODataController
    {
        [EnableQuery(PageSize = 999999)]
        public IQueryable<Product> GetProducts()
        {
            return ModelHelper.CreateRandomProducts().AsQueryable();
        }

        [EnableQuery(PageSize = 999999, MaxAnyAllExpressionDepth = 3)]
        public IEnumerable<Movie> GetMovies()
        {
            return ModelHelper.CreateMovieData();
        }

        [EnableQuery(PageSize = 999999, MaxAnyAllExpressionDepth = 3)]
        public IEnumerable<Movie> GetMoviesBig()
        {
            return ModelHelper.CreateMovieBigData();
        }

        [EnableQuery(PageSize = 999999)]
        public ITestActionResult GetProductsHttpResponse()
        {
            return Ok<IEnumerable<Product>>(GetProducts());
        }

        [EnableQuery(PageSize = 999999)]
        public Task<IEnumerable<Product>> GetAsyncProducts()
        {
            return Task.Factory.StartNew<IEnumerable<Product>>(() =>
            {
                return GetProducts();
            });
        }

        public IEnumerable<Customer> GetProductsByOptions(ODataQueryOptions options)
        {
            var products = this.GetProducts().AsQueryable();

            return options.ApplyTo(products) as IQueryable<Customer>;
        }

        [EnableQuery(PageSize = 999999)]
        public IQueryable GetProductsAsAnonymousType()
        {
            var products = this.GetProducts().Select(p =>
                new
                {
                    ID = p.ID,
                    Name = p.Name,
                    PublishDate = p.PublishDate,
                    ReleaseDate = p.ReleaseDate,
                    Date = p.Date,
                    NullableDate = p.NullableDate,
                    TimeOfDay = p.TimeOfDay,
                    NullableTimeOfDay = p.NullableTimeOfDay,
                    Price = p.Price,
                    DiscontinuedDate = p.DiscontinuedDate,
                    Taxable = p.Taxable,
                    DateTimeOffset = p.DateTimeOffset,
                    @Supplier = p.Supplier
                }).AsQueryable();

            return products;
        }
    }
}
