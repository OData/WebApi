using System.Reflection;
using System.Web.Http.Controllers;

namespace System.Web.Http
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NonActionAttribute : Attribute, IActionMethodSelector
    {
        bool IActionMethodSelector.IsValidForRequest(HttpControllerContext controllerContext, MethodInfo methodInfo)
        {
            return false;
        }
    }
}
