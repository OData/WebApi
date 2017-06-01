using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.OData;
using Microsoft.Framework.DependencyInjection;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
    [EnableQuery]
    [Route("odata/Products")]
    public class ProductsController : Controller
    {
        private readonly SampleContext _sampleContext;

        public ProductsController(SampleContext sampleContext)
        {
            _sampleContext = sampleContext;
        }

        // GET: api/Products
        [HttpGet]
        public IEnumerable<Product> Get()
        {
            return _sampleContext.Products;
        }

        // GET api/Products/5
        [HttpGet("{id}")]
        public IActionResult Get(int productId)
        {
            var product = _sampleContext.FindProduct(productId);
            if (product == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(product);
        }

        [HttpGet("{id}/Name")]
        public IActionResult GetName(int productId)
        {
            var product = _sampleContext.FindProduct(productId);
            if (product == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(product.Name);
        }

        [HttpGet("{id}/Price")]
        public IActionResult GetPrice(int productId)
        {
            var product = _sampleContext.FindProduct(productId);
            if (product == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(product.Price);
        }

        [HttpGet("{id}/ProductId")]
        public IActionResult GetProductId(int productId)
        {
            var product = _sampleContext.FindProduct(productId);
            if (product == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(product.ProductId);
        }

        // POST api/Products
        [HttpPost]
        public IActionResult Post([FromBody]Product value)
        {
            var locationUri = $"http://localhost:9091/api/Products/{value.ProductId}";
            return Created(locationUri, _sampleContext.AddProduct(value));
        }

        // PUT api/Products/5
        [HttpPut("{id}")]
        public IActionResult Put(int productId, [FromBody]Product value)
        {
            if (!_sampleContext.UpdateProduct(productId, value))
            {
                return HttpNotFound();
            }

            return new NoContentResult();
        }

        // DELETE api/Products/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int productId)
        {
            if (!_sampleContext.DeleteProduct(productId))
            {
                return HttpNotFound();
            }

            return new NoContentResult();
        }
    }
}
