using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.OData;
using Microsoft.Extensions.DependencyInjection;
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

        [HttpGet("MostExpensive")]
        public IActionResult MostExpensive()
        {
            var product = _sampleContext.Products.Max(x => x.Price);
            return Ok(product);
        }

        // GET: api/Products
        [HttpGet]
        public IActionResult Get()
        {
            if (Request.Path.Value.EndsWith(".MostExpensive"))
            {
                return MostExpensive();
            }
            return Ok(_sampleContext.Products);
        }

        // GET api/Products/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var product = _sampleContext.FindProduct(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(product);
        }

        [HttpGet("{id}/Name")]
        public IActionResult GetName(int id)
        {
            var product = _sampleContext.FindProduct(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(product.Name);
        }

        [HttpGet("{id}/Namex")]
        public IActionResult GetNamex(int id)
        {
            var product = _sampleContext.FindProduct(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(product.Name);
        }

        [HttpGet("{id}/Price")]
        public IActionResult GetPrice(int id)
        {
            var product = _sampleContext.FindProduct(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            return new ObjectResult(product.Price);
        }

        [HttpGet("{id}/ProductId")]
        public IActionResult GetProductId(int id)
        {
            var product = _sampleContext.FindProduct(id);
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
