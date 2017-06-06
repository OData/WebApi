using System;

namespace Nuwa
{
    /// <summary>
    /// Mark the method which is used to adjust HttpConfiguration
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NuwaConfigurationAttribute : Attribute
    {
    }
}