using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
	[EnableQuery]
	[Route("odata/Roles")]
	public class RolesController : ODataCrudController<IdentityRole, string>
	{
		public RolesController(IEdmModel model, ISampleService sampleService) : base(
			model,
			new CrudBase<IdentityRole, string>(sampleService as DbContext, (sampleService as ApplicationDbContext).Roles,
				entity => entity.Id)
			)
		{
		}
	}
}