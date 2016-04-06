using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
    [EnableQuery]
    [Route("odata/Products")]
    //[EnableCors("AllowAll")]
    public class ProductsController : ODataCrudController<Product, int>
	{
        private readonly ISampleService _sampleService;

        // This is needed to prevent action resolution issues
        [HttpGet("MostExpensive")]
        public IActionResult MostExpensive()
        {
            var product = _sampleService.Products.Max(x => x.Price);
            return Ok(product);
        }

        // This is needed to prevent action resolution issues
        [HttpGet("MostExpensive2")]
        public IActionResult MostExpensive2()
        {
            var value = _sampleService.Products.Max(x => x.Price);
            return Ok(value * 2);
        }

        [HttpGet("{id}/ShortName")]
        public IActionResult ShortName(int id)
        {
            return Ok(_sampleService.Products.Single(p => p.ProductId == id).Name.Substring(0, 4));
        }

        [HttpGet("{id}/PrintName(prefix={prefix})")]
        public IActionResult PrintName(int id, string prefix)
        {
            return Ok($"{prefix}: {_sampleService.Products.Single(p => p.ProductId == id).Name}");
        }

		// GET: api/Products
		[PageSize(5)]
		public override async Task<IQueryable<Product>> Get()
		{
			return await base.Get();
	    }

        [HttpGet("{id}/Name")]
        public IActionResult GetName(int id)
        {
            var product = _sampleService.FindProduct(id);
            if (product == null)
            {
                return NotFound();
            }

            return new ObjectResult(product.Name);
        }

        [HttpGet("{id}/Namex")]
        public IActionResult GetNamex(int id)
        {
            var product = _sampleService.FindProduct(id);
            if (product == null)
            {
                return NotFound();
            }

            return new ObjectResult(product.Name);
        }

        [HttpGet("{id}/Price")]
        public IActionResult GetPrice(int id)
        {
            var product = _sampleService.FindProduct(id);
            if (product == null)
            {
                return NotFound();
            }

            return new ObjectResult(product.Price);
        }

        [HttpGet("{id}/ProductId")]
        public IActionResult GetProductId(int id)
        {
            var product = _sampleService.FindProduct(id);
            if (product == null)
            {
                return NotFound();
            }

            return new ObjectResult(product.ProductId);
        }

	    public ProductsController(ISampleService sampleService) : base(
			new CrudBase<Product, int>(sampleService as DbContext, (sampleService as ApplicationDbContext).Products, product => product.ProductId)
			)
	    {
		    _sampleService = sampleService;
	    }
	}
}
