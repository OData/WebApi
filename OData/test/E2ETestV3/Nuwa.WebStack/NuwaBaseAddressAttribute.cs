using System;
using System.Reflection;

namespace Nuwa
{
    /// <summary>
    /// NuwaBaseAddress attribute is used to mark a property to which the Nuwa will
    /// assign base address of the host setup.
    /// 
    /// The marked element must be a string type public property, which both get and
    /// set accessor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NuwaBaseAddressAttribute : Attribute
    {
        public static bool Verify(PropertyInfo property)
        {
            if (!property.PropertyType.IsAssignableFrom(typeof(string)))
            {
                return false;
            }

            if (!property.CanWrite)
            {
                return false;
            }

            return true;
        }
    }
}