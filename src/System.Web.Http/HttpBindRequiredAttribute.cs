using System.Web.Http.ModelBinding;

namespace System.Web.Http
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class HttpBindRequiredAttribute : HttpBindingBehaviorAttribute
    {
        public HttpBindRequiredAttribute()
            : base(HttpBindingBehavior.Required)
        {
        }
    }
}
