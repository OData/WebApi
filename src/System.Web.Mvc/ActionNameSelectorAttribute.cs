using System.Reflection;

namespace System.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class ActionNameSelectorAttribute : Attribute
    {
        public abstract bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo);
    }
}
