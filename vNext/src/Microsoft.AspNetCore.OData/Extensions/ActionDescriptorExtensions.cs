using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Query;

namespace Microsoft.AspNetCore.OData.Extensions
{
	public static class ActionDescriptorExtensions
	{
		public static bool HasQueryOption(this ActionDescriptor actionDescriptor)
		{
			return actionDescriptor.PageSize().HasValue;
		}

		public static int? PageSize(this ActionDescriptor actionDescriptor)
		{
			int? pageSize = null;
			var controllerActionDescriptor = actionDescriptor as ControllerActionDescriptor;
			var pageSizeAttribute = controllerActionDescriptor?.MethodInfo.GetCustomAttribute<PageSizeAttribute>();
			if (pageSizeAttribute != null)
			{
				pageSize = pageSizeAttribute.Value;
			}
			return pageSize;
		}
	}
}