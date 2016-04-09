using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;

namespace ODataSample.Web.Controllers
{
	public abstract class ODataCrudController<T, TKey> : Controller
		where T : class
	{
		public IEdmModel Model { get; set; }
		protected readonly CrudBase<T, TKey> Crud;

		protected ODataCrudController(IEdmModel model, CrudBase<T, TKey> crud)
		{
			Model = model;
			Crud = crud;
		}

		// GET: api/Products
		[System.Web.Mvc.HttpGet]
		public virtual async Task<IQueryable<T>> Get()
		{
			return Crud.All();
		}

		//[System.Web.Mvc.HttpGet("{id}")]
		[System.Web.Mvc.HttpGet]
		public virtual async Task<ActionResult> Get(TKey id)
		{
			var entity = Crud.Find(id);
			if (entity == null)
			{
				return HttpNotFound();
			}

			return new JsonResult() { Data = entity };
		}

		// POST api/[Entities]
		[System.Web.Mvc.HttpPost]
		public virtual async Task<ActionResult> Post([FromBody] T value)
		{
			// For legibility
			var req = HttpContext.Request;
			//var locationUri = $"{req.Url.Scheme}://{req.Host}/{req.Path}/{Crud.EntityId(value)}";
			await Crud.AddAndSaveAsync(value);
			return new EmptyResult();
		}

		public virtual async Task OnBeforePatchAsync(TKey id, T entity, T patchEntity, JObject jObject)
		{
		}

		public virtual async Task OnAfterPatchAsync(TKey id, T entity, T patchEntity, JObject jObject)
		{
		}

		public virtual async Task<ActionResult> Patch(TKey id, T entity, JObject value)
		{
			var patchEntity = value.ToObject<T>();
			await OnBeforePatchAsync(id, entity, patchEntity, value);
			foreach (var property in value)
			{
				var propertyInfo = entity.GetType().GetTypeInfo().GetProperty(property.Key);
				//var entityType = Model.GetEdmType(propertyInfo.DeclaringType) as EdmEntityType;
				//var propertyConfiguration = entityType?.FindProperty(propertyInfo.Name) as PropertyConfiguration;
				//if (propertyConfiguration != null && !propertyConfiguration.IsIgnored)
				{
					// Set the value to the value of the same property on the patch entity
					propertyInfo?.SetValue(entity, propertyInfo.GetValue(patchEntity));
				}
			}
			await OnAfterPatchAsync(id, entity, patchEntity, value);
			if (!await Crud.UpdateAndSaveAsync(entity))
			{
				return HttpNotFound();
			}
			return new EmptyResult();
		}

		// PATCH api/[Entities]/5
		//[System.Web.Mvc.HttpPatch("{id}")]
		[System.Web.Mvc.HttpPatch]
		public virtual async Task<ActionResult> Patch(TKey id, [FromBody] JObject value)
		{
			var entity = Crud.Find(id);
			return await Patch(id, entity, value);
		}

		// PATCPUT api/[Entities]/5
		//[HttpPut("{id}")]
		[System.Web.Http.HttpPut]
		public virtual async Task<ActionResult> Put(TKey id, [FromBody] JObject value)
		{
			return await Patch(id, value);
		}

		// DELETE api/Products/5
		[System.Web.Http.HttpDelete]
		//[HttpDelete("{id}")]
		public virtual async Task<ActionResult> Delete(TKey id)
		{
			if (!await Crud.DeleteAndSaveAsync(id))
			{
				return HttpNotFound();
			}

			return new EmptyResult();
		}
	}
}