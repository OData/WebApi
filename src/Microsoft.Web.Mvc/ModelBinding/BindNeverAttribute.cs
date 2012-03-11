using System;

namespace Microsoft.Web.Mvc.ModelBinding
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class BindNeverAttribute : BindingBehaviorAttribute
    {
        public BindNeverAttribute()
            : base(BindingBehavior.Never)
        {
        }
    }
}
