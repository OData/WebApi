using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
	[EnableQuery]
	[Route("odata/Orders")]
	public class OrdersController : ODataCrudController<Order, string>
	{
		public OrdersController(IEdmModel model, ISampleService sampleService) : base(
			model,
			new CrudBase<Order, string>(sampleService as DbContext, (sampleService as ApplicationDbContext).Orders,
				entity => entity.Id)
			)
		{
		}
	}
}