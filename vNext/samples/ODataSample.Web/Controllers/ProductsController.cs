using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
	[EnableQuery]
	[Route("odata/Products")]
	//[EnableCors("AllowAll")]
	public class ProductsController : ODataCrudController<Product, int>
	{
		private readonly ISampleService _sampleService;

		public ProductsController(IEdmModel model, ISampleService sampleService) : base(
			model,
			new CrudBase<Product, int>(sampleService as DbContext, (sampleService as ApplicationDbContext).Products,
				product => product.ProductId)
			)
		{
			_sampleService = sampleService;
		}

		[HttpGet("{id}/DuplicateMethodName")]
		public IActionResult DuplicateMethodName(int id)
		{
			return Ok($"Hello from product {id}");
		}

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
			return Ok(value*2);
		}

		[HttpGet("{id}/ShortName")]
		public IActionResult ShortName(int id)
		{
			return Ok(_sampleService.Products.Single(p => p.ProductId == id).Name.Substring(0, 4));
		}

		[HttpGet("{id}/GetName(prefix={prefix})")]
		public IActionResult GetName(int id, string prefix)
		{
			return Ok($"{prefix}: {_sampleService.Products.Single(p => p.ProductId == id).Name}");
		}

		[HttpPost("{id}/PostName")]
		public IActionResult PostName(int id, [FromBody] JToken prefix)
		{
			return GetName(id, prefix["prefix"].Value<string>());
		}

		// GET: api/Products
		//[PageSize(4)]
		public override async Task<IQueryable<Product>> Get()
		{
			var db = _sampleService as ApplicationDbContext;
			var query = db.Users.Select(var1 => new //SelectExpandBinder.SelectAllAndExpand<ApplicationUser>()
			{
				ModelID = "788080e9-dd2b-4531-940f-da126125c157",
				User = var1,
				Container = new //PropertyContainer.SingleExpandedProperty<SelectExpandBinder.SelectAll<Product>>()
				{
					Name = "FavouriteProduct",
					Value = new //SelectExpandBinder.SelectAll<Product>()
					{
						ModelID = "788080e8-dd2b-4531-940f-da126125c157",
						FavProd = var1.FavouriteProduct
					},
					//IsNull = Equals(var1.Customer, null)
				}
			});
			var elms = query.ToList();
			//IQueryable<Product> pp;
			//pp.SelectAndExpand()
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

		public override Task<IActionResult> OnPost(Product model)
		{
			if (model != null && model.Name == "aaa")
			{
				ModelState.AddModelError("Name", "Nope, no \"aaa\"");
			}
			return base.OnPost(model);
		}
	}
}