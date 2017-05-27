using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
	[EnableQuery]
	[Route("odata/Orders")]
	public class OrdersController : ODataCrudController<Order, int>
	{
		public OrdersController(IEdmModel model, ISampleService sampleService) : base(
			model,
			new CrudBase<Order, int>(sampleService as DbContext, (sampleService as ApplicationDbContext).Orders,
				entity => entity.Id)
			)
		{
		}

		[HttpGet("{id}/DuplicateMethodName")]
		public IActionResult DuplicateMethodName(int id)
		{
			return Ok($"Hello from order {id}");
		}
	}
}