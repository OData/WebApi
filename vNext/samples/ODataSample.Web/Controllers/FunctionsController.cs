using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using ODataSample.Web.Models;

namespace ODataSample.Web.Controllers
{
	[EnableQuery]
	[Route("odata")]
	public class FunctionsController : Controller
	{
		[HttpGet("odata/HelloWorld")]
		public IActionResult HelloWorld()
		{
			double rate = 5.6;  // Use a fake number for the sample.
			return Ok(rate);
		}

		[HttpGet("odata/HelloComplexWorld")]
		public IActionResult HelloComplexWorld()
		{
			return Ok(new Permissions(true, false, true, false));
		}

		[HttpGet("odata/Multiply(a={a},b={b})")]
		public IActionResult Multiply([FromUri]int a, [FromUri]int b)
		{
			return Ok(a * b);
		}
	}
}