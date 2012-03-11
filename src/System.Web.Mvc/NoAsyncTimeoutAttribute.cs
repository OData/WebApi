using System.Threading;

namespace System.Web.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class NoAsyncTimeoutAttribute : AsyncTimeoutAttribute
    {
        public NoAsyncTimeoutAttribute()
            : base(Timeout.Infinite)
        {
        }
    }
}
