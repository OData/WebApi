using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View(new LoginViewModel());
		}

		public IActionResult Index2()
		{
			return Content("Hey ho");
		}



		[AllowAnonymous]
		[HttpGet]
		public bool IsAuthenticated()
		{
			return User.Identity.IsAuthenticated;
		}

		[AllowAnonymous]
		[HttpGet("~/seed")]
		public async Task<IActionResult> Seed()
		{
			await new Seeder(HttpContext.RequestServices).EnsureDatabaseAsync();
			return Ok("Seeded");
		}
	}
}