using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
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
		[HttpGet]
		public virtual async Task<IQueryable<T>> Get()
		{
			return Crud.All();
		}

		[HttpGet("{id}")]
		public virtual async Task<IQueryable<T>> Get(TKey id)
		{
			var entity = Crud.FindQuery(id);
			if (entity == null)
			{
				return new EnumerableQuery<T>(new List<T>());
			}

			return entity;
		}

		// POST api/[Entities]
		[HttpPost]
		public virtual async Task<IActionResult> Post([FromBody] T value)
		{
			// For legibility
			var req = HttpContext.Request;
			var locationUri = $"{req.Protocol}://{req.Host}/{req.Path}/{Crud.EntityId(value)}";
			return Created(locationUri, await Crud.AddAndSaveAsync(value));
		}

		public virtual async Task OnBeforePatchAsync(TKey id, T entity, T patchEntity, JObject jObject)
		{
		}

		public virtual async Task OnAfterPatchAsync(TKey id, T entity, T patchEntity, JObject jObject)
		{
		}

		public virtual async Task<IActionResult> Patch(TKey id, T entity, JObject value)
		{
			var patchEntity = value.ToObject<T>();
			await OnBeforePatchAsync(id, entity, patchEntity, value);
			foreach (var property in value)
			{
				var propertyInfo = entity.GetType().GetTypeInfo().GetProperty(property.Key);
				var entityType = Model.GetEdmType(propertyInfo.DeclaringType) as EdmEntityType;
				var propertyConfiguration = entityType?.FindProperty(propertyInfo.Name) as PropertyConfiguration;
				if (propertyConfiguration != null && !propertyConfiguration.IsIgnored)
				{
					// Set the value to the value of the same property on the patch entity
					propertyInfo?.SetValue(entity, propertyInfo.GetValue(patchEntity));
				}
			}
			await OnAfterPatchAsync(id, entity, patchEntity, value);
			if (!await Crud.UpdateAndSaveAsync(entity))
			{
				return NotFound();
			}
			return new NoContentResult();
		}

		// PATCH api/[Entities]/5
		[HttpPatch("{id}")]
		public virtual async Task<IActionResult> Patch(TKey id, [FromBody] JObject value)
		{
			var entity = Crud.Find(id);
			return await Patch(id, entity, value);
		}

		// PATCPUT api/[Entities]/5
		[HttpPut("{id}")]
		public virtual async Task<IActionResult> Put(TKey id, [FromBody] JObject value)
		{
			return await Patch(id, value);
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