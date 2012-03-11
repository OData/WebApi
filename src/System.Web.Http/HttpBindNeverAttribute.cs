namespace System.Web.Http
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class HttpBindNeverAttribute : HttpBindingBehaviorAttribute
    {
        public HttpBindNeverAttribute()
            : base(HttpBindingBehavior.Never)
        {
        }
    }
}
