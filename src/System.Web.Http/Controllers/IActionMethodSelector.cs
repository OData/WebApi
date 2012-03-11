using System.Reflection;

namespace System.Web.Http.Controllers
{
    internal interface IActionMethodSelector
    {
        bool IsValidForRequest(HttpControllerContext controllerContext, MethodInfo methodInfo);
    }
}
