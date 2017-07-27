using System;

namespace Nuwa
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class NuwaTestControllerAttribute : Attribute
    {
        public NuwaTestControllerAttribute(Type type)
        {
            this.ControllerType = type;
        }

        public Type ControllerType { get; private set; }
    }
}