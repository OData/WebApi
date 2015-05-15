using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.DependencyInjection;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
    [Route("api/Products")]
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
        public IActionResult Get(int id)
        {
            return Json(_sampleContext.Products.Single(p => p.ProductId == id));
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
        public IActionResult Put(int id, [FromBody]Product value)
        {
            if (!_sampleContext.UpdateProduct(id, value))
            {
                return HttpNotFound();
            }

            return new NoContentResult();
        }

        // DELETE api/Products/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (!_sampleContext.DeleteProduct(id))
            {
                return HttpNotFound();
            }

            return new NoContentResult();
        }
    }
}
