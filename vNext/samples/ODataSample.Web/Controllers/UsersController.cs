using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
	[EnableQuery]
	[Route("odata/Users")]
	//[EnableCors("AllowAll")]
	public class UsersController : ODataCrudController<ApplicationUser, string>
	{
		public UsersController(IEdmModel model, ApplicationDbContext context) : base(
			model, new CrudBase<ApplicationUser, string>(context, context.Users, u => u.Id))
		{
		}
	}
}