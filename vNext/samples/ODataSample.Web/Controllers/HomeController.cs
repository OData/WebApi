using Microsoft.AspNetCore.Mvc;

namespace ODataSample.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}