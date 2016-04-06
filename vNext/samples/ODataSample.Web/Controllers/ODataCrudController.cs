using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ODataSample.Web.Controllers
{
	public abstract class ODataCrudController<T, TKey> : Controller
		where T : class
	{
		protected readonly CrudBase<T, TKey> Crud;

		protected ODataCrudController(CrudBase<T, TKey> crud)
		{
			Crud = crud;
		}

		// GET: api/Products
		[HttpGet]
		public virtual async Task<IQueryable<T>> Get()
		{
			return Crud.All();
		}

		[HttpGet("{id}")]
		public virtual async Task<IActionResult> Get(TKey id)
		{
			var entity = Crud.Find(id);
			if (entity == null)
			{
				return NotFound();
			}

			return new ObjectResult(entity);
		}

		// POST api/[Entities]
		[HttpPost]
		public virtual async Task<IActionResult> Post([FromBody]T value)
		{
			// For legibility
			var req = HttpContext.Request;
			await Crud.AddAndSaveAsync(value);
			var locationUri = $"{req.Protocol}://{req.Host}/{req.Path}/{Crud.EntityId(value)}";
			return Created(locationUri, Crud.AddAndSaveAsync(value));
		}

		// PUT api/[Entities]/5
		[HttpPut("{id}")]
		public virtual async Task<IActionResult> Put(int id, [FromBody]T value)
		{
			if (!await Crud.UpdateAndSaveAsync(value))
			{
				return NotFound();
			}

			return new NoContentResult();
		}

		// DELETE api/Products/5
		[HttpDelete("{id}")]
		public virtual async Task<IActionResult> Delete(TKey id)
		{
			if (!await Crud.DeleteAndSaveAsync(id))
			{
				return NotFound();
			}

			return new NoContentResult();
		}
	}
}