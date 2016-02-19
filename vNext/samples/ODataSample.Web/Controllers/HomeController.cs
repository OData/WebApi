using Microsoft.AspNetCore.Mvc;

namespace ODataSample.Web.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("~/Index")]
        public IActionResult Index()
        {
            return Ok("Hello, world");
        }
    }
}