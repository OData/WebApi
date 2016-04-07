using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
	[EnableQuery]
	[Route("odata/Users")]
	//[EnableCors("AllowAll")]
	public class UsersController : ODataCrudController<ApplicationUser, string>
	{
		public UsersController(ApplicationDbContext context) : base(new CrudBase<ApplicationUser, string>(context, context.Users, u => u.Id))
		{
		}
	}
}