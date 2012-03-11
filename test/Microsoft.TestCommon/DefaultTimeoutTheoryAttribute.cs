using System;
using Xunit.Extensions;

namespace Microsoft.TestCommon
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DefaultTimeoutTheoryAttribute : TheoryAttribute
    {
        public DefaultTimeoutTheoryAttribute()
        {
            Timeout = TimeoutConstant.DefaultTimeout;
        }
    }
}
