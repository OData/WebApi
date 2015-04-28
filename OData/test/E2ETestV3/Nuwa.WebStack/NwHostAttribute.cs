using System;

namespace Nuwa
{
    /// <summary>
    /// Defines the host strategy of attributed test class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class NwHostAttribute : Attribute
    {
        public NwHostAttribute(HostType type)
        {
            this.HostType = type;
        }

        public HostType HostType
        {
            get;
            private set;
        }
    }
}