using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using WebStack.QA.Test.OData.SxS2.ODataV4.Models;

namespace WebStack.QA.Test.OData.SxS2.ODataV4.Controllers
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
