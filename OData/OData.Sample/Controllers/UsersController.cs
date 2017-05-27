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
	public class UsersController : ODataController
	{
		ApplicationDbContext db = new ApplicationDbContext();
		[EnableQuery]
		public IQueryable<ApplicationUser> Get()
		{
			return db.Users;
		}
		[EnableQuery]
		public SingleResult<ApplicationUser> Get([FromODataUri] string key)
		{
			IQueryable<ApplicationUser> result = db.Users.Where(p => p.Id == key);
			return SingleResult.Create(result);
		}
		public async Task<IHttpActionResult> Post(ApplicationUser ApplicationUser)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			db.Users.Add(ApplicationUser);
			await db.SaveChangesAsync();
			return Created(ApplicationUser);
		}
		public async Task<IHttpActionResult> Patch([FromODataUri] string key, Delta<ApplicationUser> ApplicationUser)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var entity = db.Users.Find(key);
			if (entity == null)
			{
				return NotFound();
			}
			ApplicationUser.Patch(entity);
			try
			{
				await db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ApplicationUserExists(key))
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
		public async Task<IHttpActionResult> Put([FromODataUri] string key, ApplicationUser update)
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
				if (!ApplicationUserExists(key))
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
		public async Task<IHttpActionResult> Delete([FromODataUri] string key)
		{
			var ApplicationUser = db.Users.Find(key);
			if (ApplicationUser == null)
			{
				return NotFound();
			}
			db.Users.Remove(ApplicationUser);
			await db.SaveChangesAsync();
			return StatusCode(HttpStatusCode.NoContent);
		}
		private bool ApplicationUserExists(string key)
		{
			return db.Users.Any(p => p.Id == key);
		}
		protected override void Dispose(bool disposing)
		{
			db.Dispose();
			base.Dispose(disposing);
		}
	}
}