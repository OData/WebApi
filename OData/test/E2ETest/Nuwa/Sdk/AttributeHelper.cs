using System;
using System.Collections.Generic;
using Xunit.Sdk;

namespace Nuwa.Sdk
{
    public static class AttributeHelper
    {
        /// <summary>
        /// Return the first found custom attribute on a method
        /// </summary>
        /// <param name="attributeType">type of the custom attribute</param>
        /// <returns>the first found custom attribute; or null none is found</returns>
        public static T GetFirstCustomAttribute<T>(this IMethodInfo me) where T : Attribute
        {
            IAttributeInfo attrInfo = null;

            foreach (var a in me.GetCustomAttributes(typeof(T)))
            {
                attrInfo = a;
                break;
            }

            return attrInfo != null ? attrInfo.GetInstance<T>() : null;
        }

        /// <summary>
        /// Return the first found custom attribute
        /// </summary>
        /// <param name="attributeType">type of the custom attribute</param>
        /// <returns>the first found custom attribute; or null none is found</returns>
        public static IAttributeInfo GetFirstCustomAttribute(this ITypeInfo me, Type attributeType)
        {
            IAttributeInfo retval = null;

            foreach (var a in me.GetCustomAttributes(attributeType))
            {
                retval = a;
                break;
            }

            return retval;
        }

        /// <summary>
        /// Return the first found custom attribute
        /// </summary>
        /// <param name="attributeType">type of the custome attribute</param>
        /// <returns>the first found custom attribute; or null none is found</returns>
        public static T GetFirstCustomAttribute<T>(this ITypeInfo me) where T : Attribute
        {
            IAttributeInfo attrInfo = null;

            foreach (var a in me.GetCustomAttributes(typeof(T)))
            {
                attrInfo = a;
                break;
            }

            if (attrInfo != null)
            {
                return attrInfo.GetInstance<T>();
            }
            else
            {
                return null;
            }
        }

        public static T[] GetCustomAttributes<T>(this ITypeInfo me) where T : Attribute
        {
            var retvals = new List<T>();

            foreach (var a in me.GetCustomAttributes(typeof(T)))
            {
                var attr = a.GetInstance<T>();
                if (attr != null)
                {
                    retvals.Add(attr);
                }
            }

            return retvals.ToArray();
        }

        /// <summary>
        /// Returns the methods marked by given type of attribute
        /// </summary>
        /// <param name="me">this</param>
        /// <param name="attribute">target attribute type</typeparam>
        /// <returns>Array of the found method, return zero length array if nothing is found.</returns>
        public static IMethodInfo[] GetMethodMarkedByAttribute(this ITypeInfo me, Type attribute)
        {
            var retval = new List<IMethodInfo>();

            foreach (var m in me.GetMethods())
            {
                if (m.HasAttribute(attribute))
                {
                    retval.Add(m);
                }
            }

            return retval.ToArray();
        }
    }
}
