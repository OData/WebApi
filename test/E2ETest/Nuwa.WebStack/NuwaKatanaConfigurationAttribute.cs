using System;

namespace Nuwa
{
    /// <summary>
    /// Mark the method which is used to adjust IAppBuilder
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class NuwaKatanaConfigurationAttribute : Attribute
    {
    }
}