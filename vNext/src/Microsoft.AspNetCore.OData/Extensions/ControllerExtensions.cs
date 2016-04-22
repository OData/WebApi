using System.Web.Http;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.OData.Extensions
{
	public static class ControllerExtensions
	{
		public static IActionResult ODataModelStateError(this Controller controller)
		{
			return controller.BadRequest(new HttpError(controller.ModelState, false).CreateODataError());
		}
	}
}