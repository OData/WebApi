using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
	public class OrdersController : ODataController
	{
		ApplicationDbContext db = new ApplicationDbContext();
		[EnableQuery]
		public IQueryable<Order> Get()
		{
			return db.Orders;
		}
		[EnableQuery]
		public SingleResult<Order> Get([FromODataUri] string key)
		{
			IQueryable<Order> result = db.Orders.Where(p => p.Id == key);
			return SingleResult.Create(result);
		}
		public async Task<IHttpActionResult> Post(Order Order)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			db.Orders.Add(Order);
			await db.SaveChangesAsync();
			return Created(Order);
		}
		public async Task<IHttpActionResult> Patch([FromODataUri] string key, Delta<Order> Order)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var entity = await db.Orders.FindAsync(key);
			if (entity == null)
			{
				return NotFound();
			}
			Order.Patch(entity);
			try
			{
				await db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!OrderExists(key))
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
		public async Task<IHttpActionResult> Put([FromODataUri] string key, Order update)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			if (key != update.Id)
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
				if (!OrderExists(key))
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
			var Order = await db.Orders.FindAsync(key);
			if (Order == null)
			{
				return NotFound();
			}
			db.Orders.Remove(Order);
			await db.SaveChangesAsync();
			return StatusCode(HttpStatusCode.NoContent);
		}
		private bool OrderExists(string key)
		{
			return db.Orders.Any(p => p.Id == key);
		}
		protected override void Dispose(bool disposing)
		{
			db.Dispose();
			base.Dispose(disposing);
		}
	}
}