using System;
using Nuwa.Control;
using Xunit;

namespace Nuwa
{
    /// <summary>
    /// NuwaFrameworkAttribute is used to mark a Nuwa test class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NuwaFrameworkAttribute : RunWithAttribute
    {
        public NuwaFrameworkAttribute()
            : base(typeof(NuwaTestClassCommand))
        {
        }
    }
}
