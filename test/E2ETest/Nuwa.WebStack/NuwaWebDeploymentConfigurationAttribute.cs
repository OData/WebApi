using System;

namespace Nuwa
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class NuwaWebDeploymentConfigurationAttribute : Attribute
    {
    }
}
