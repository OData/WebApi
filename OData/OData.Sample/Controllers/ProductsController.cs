using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
	public class ProductsController : ODataController
	{
		ApplicationDbContext db = new ApplicationDbContext();
		[EnableQuery]
		public IQueryable<Product> Get()
		{
			return db.Products;
		}
		[EnableQuery]
		public SingleResult<Product> Get([FromODataUri] int key)
		{
			IQueryable<Product> result = db.Products.Where(p => p.ProductId == key);
			return SingleResult.Create(result);
		}
		public async Task<IHttpActionResult> Post(Product product)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			db.Products.Add(product);
			await db.SaveChangesAsync();
			return Created(product);
		}
		public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Product> product)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var entity = await db.Products.FindAsync(key);
			if (entity == null)
			{
				return NotFound();
			}
			product.Patch(entity);
			try
			{
				await db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ProductExists(key))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}
			return Updated(entity);
		}
		public async Task<IHttpActionResult> Put([FromODataUri] int key, Product update)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			if (key != update.ProductId)
			{
				return BadRequest();
			}
			db.Entry(update).State = EntityState.Modified;
			try
			{
				await db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ProductExists(key))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}
			return Updated(update);
		}
		public async Task<IHttpActionResult> Delete([FromODataUri] int key)
		{
			var product = await db.Products.FindAsync(key);
			if (product == null)
			{
				return NotFound();
			}
			db.Products.Remove(product);
			await db.SaveChangesAsync();
			return StatusCode(HttpStatusCode.NoContent);
		}
		private bool ProductExists(int key)
		{
			return db.Products.Any(p => p.ProductId == key);
		}
		protected override void Dispose(bool disposing)
		{
			db.Dispose();
			base.Dispose(disposing);
		}
	}
	//[EnableQuery]
	//[Route("odata/Products")]
	////[EnableCors("AllowAll")]
	//public class ProductsController : ODataCrudController<Product, int>
	//{
	//	private readonly ISampleService _sampleService;

	//	public ProductsController(IEdmModel model, ISampleService sampleService) : base(
	//		model,
	//		new CrudBase<Product, int>(sampleService as DbContext, (sampleService as ApplicationDbContext).Products,
	//			product => product.ProductId)
	//		)
	//	{
	//		_sampleService = sampleService;
	//	}

	//	// This is needed to prevent action resolution issues
	//	[HttpGet("MostExpensive")]
	//	public IActionResult MostExpensive()
	//	{
	//		var product = _sampleService.Products.Max(x => x.Price);
	//		return Ok(product);
	//	}

	//	// This is needed to prevent action resolution issues
	//	[HttpGet("MostExpensive2")]
	//	public IActionResult MostExpensive2()
	//	{
	//		var value = _sampleService.Products.Max(x => x.Price);
	//		return Ok(value*2);
	//	}

	//	[HttpGet("{id}/ShortName")]
	//	public IActionResult ShortName(int id)
	//	{
	//		return Ok(_sampleService.Products.Single(p => p.ProductId == id).Name.Substring(0, 4));
	//	}

	//	[HttpGet("{id}/GetName(prefix={prefix})")]
	//	public IActionResult GetName(int id, string prefix)
	//	{
	//		return Ok($"{prefix}: {_sampleService.Products.Single(p => p.ProductId == id).Name}");
	//	}

	//	[HttpPost("{id}/PostName")]
	//	public IActionResult PostName(int id, [FromBody] JToken prefix)
	//	{
	//		return GetName(id, prefix["prefix"].Value<string>());
	//	}

	//	// GET: api/Products
	//	//[PageSize(5)]
	//	public override async Task<IQueryable<Product>> Get()
	//	{
	//		var db = _sampleService as ApplicationDbContext;
	//		var query = db.Products.Select(var1 => new SelectExpandBinder.SelectAllAndExpand<Product>()
	//		{
	//			ModelID = "788080e9-dd2b-4531-940f-da126125c157",
	//			Instance = var1
	//			,
	//			Container = new PropertyContainer.SingleExpandedProperty<SelectExpandBinder.SelectAll<Customer>>()
	//			{
	//				Name = "Customer",
	//				Value = new SelectExpandBinder.SelectAll<Customer>()
	//				{
	//					ModelID = "788080e9-dd2b-4531-940f-da126125c157",
	//					Instance = var1.Customer
	//				},
	//				IsNull = false,//var1.Customer == null
	//			}
	//		});
	//		var elms = query.ToList();
	//		//IQueryable<Product> pp;
	//		//pp.SelectAndExpand()
	//		return await base.Get();
	//	}

	//	[HttpGet("{id}/Name")]
	//	public IActionResult GetName(int id)
	//	{
	//		var product = _sampleService.FindProduct(id);
	//		if (product == null)
	//		{
	//			return NotFound();
	//		}

	//		return new ObjectResult(product.Name);
	//	}

	//	[HttpGet("{id}/Namex")]
	//	public IActionResult GetNamex(int id)
	//	{
	//		var product = _sampleService.FindProduct(id);
	//		if (product == null)
	//		{
	//			return NotFound();
	//		}

	//		return new ObjectResult(product.Name);
	//	}

	//	[HttpGet("{id}/Price")]
	//	public IActionResult GetPrice(int id)
	//	{
	//		var product = _sampleService.FindProduct(id);
	//		if (product == null)
	//		{
	//			return NotFound();
	//		}

	//		return new ObjectResult(product.Price);
	//	}

	//	[HttpGet("{id}/ProductId")]
	//	public IActionResult GetProductId(int id)
	//	{
	//		var product = _sampleService.FindProduct(id);
	//		if (product == null)
	//		{
	//			return NotFound();
	//		}

	//		return new ObjectResult(product.ProductId);
	//	}
	//}
}