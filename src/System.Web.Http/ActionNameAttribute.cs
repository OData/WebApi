using System.Web.Http.Controllers;

namespace System.Web.Http
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ActionNameAttribute : Attribute
    {
        public ActionNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
