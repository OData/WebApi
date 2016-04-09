using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
	[EnableQuery]
	[Route("odata/Customers")]
	public class CustomersController : ODataCrudController<Customer, int>
	{
		private readonly ISampleService _sampleService;

		public CustomersController(IEdmModel model, ISampleService sampleService) : base(
			model,
			new CrudBase<Customer, int>(sampleService as DbContext,
				(sampleService as ApplicationDbContext).Customers,
				customer => customer.CustomerId))
		{
			_sampleService = sampleService;
		}

		[HttpGet("{id}/FirstName")]
		public IActionResult GetFirstName(int id)
		{
			var customer = _sampleService.FindCustomer(id);
			if (customer == null)
			{
				return NotFound();
			}

			return new ObjectResult(customer.FirstName);
		}

		[HttpGet("{id}/LastName")]
		public IActionResult GetLastName(int id)
		{
			var customer = _sampleService.FindCustomer(id);
			if (customer == null)
			{
				return NotFound();
			}

			return new ObjectResult(customer.LastName);
		}

		[HttpGet("{id}/CustomerId")]
		public IActionResult GetCustomerId(int id)
		{
			var customer = _sampleService.FindCustomer(id);
			if (customer == null)
			{
				return NotFound();
			}

			return new ObjectResult(customer.CustomerId);
		}

		[HttpGet("{id}/Products")]
		public IActionResult GetProducts(int id)
		{
			var customer = _sampleService.FindCustomer(id);
			if (customer == null)
			{
				return NotFound();
			}

			return new ObjectResult(customer.Products);
		}

		public override Task<IQueryable<Customer>> Get()
		{
			//var db = _sampleService as ApplicationDbContext;
			//var query = db.Customers.Select(var1 => new SelectExpandBinder.SelectAllAndExpand<Customer>()
			//{
			//	ModelID = "29beea7c-dd56-427f-b192-79969ad77c5f",
			//	Instance = var1
			//	,
			//	Container = new PropertyContainer.NamedProperty<IEnumerable<SelectExpandBinder.SelectAll<Product>>>()
			//	{
			//		Name = "Products",
			//		Value = //new List<SelectExpandBinder.SelectAll<Product>>()
			//		//(var1 == null ? null : var1.Products) == null ? new List<SelectExpandBinder.SelectAll<Product>>() : (var1 == null ? null : var1.Products).Select(p => new SelectExpandBinder.SelectAll<Product>()),
			//		//(var1 == null ? null : var1.Products) == null ? new List<SelectExpandBinder.SelectAll<Product>>() : var1.Products.Select(p => new SelectExpandBinder.SelectAll<Product>()),
			//		var1.Products.Select(p => new SelectExpandBinder.SelectAll<Product>())
			//	}
			//});
			//var elms = query.ToList();
			return base.Get();
		}
	}
}