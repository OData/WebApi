// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Web.Http.Data
{
    internal static class TypeUtility
    {
        public static Type GetElementType(Type type)
        {
            // Array, pointers, etc.
            if (type.HasElementType)
            {
                return type.GetElementType();
            }

            // IEnumerable<T> returns T
            Type ienum = FindIEnumerable(type);
            if (ienum != null)
            {
                Type genericArg = ienum.GetGenericArguments()[0];
                return genericArg;
            }

            return type;
        }

        internal static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
            {
                return null;
            }
            if (seqType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            }
            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(seqType))
                    {
                        return ienum;
                    }
                }
            }
            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null)
                    {
                        return ienum;
                    }
                }
            }
            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
            }
            return null;
        }

        /// <summary>
        /// Returns true if the specified PropertyDescriptor is publically
        /// exposed, based on DataContract visibility rules.
        /// </summary>
        internal static bool IsDataMember(PropertyDescriptor pd)
        {
            AttributeCollection attrs = pd.ComponentType.Attributes();

            if (attrs[typeof(DataContractAttribute)] != null)
            {
                if (pd.Attributes[typeof(DataMemberAttribute)] == null)
                {
                    return false;
                }
            }
            else
            {
                if (pd.Attributes[typeof(IgnoreDataMemberAttribute)] != null)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Obtains the set of known types from the <see cref="KnownTypeAttribute"/> custom attributes
        /// attached to the specified <paramref name="type"/>.
        /// </summary>
        /// <remarks>
        /// This utility function either retrieving the declared types or invokes the method declared in <see cref="KnownTypeAttribute.MethodName"/>.
        /// </remarks>
        /// <param name="type">The type to examine for <see cref="KnownTypeAttribute"/>s</param>
        /// <param name="inherit"><c>true</c> to allow inheritance of <see cref="KnownTypeAttribute"/> from the base.</param>
        /// <returns>The distinct set of types fould via the <see cref="KnownTypeAttribute"/>s</returns>
        internal static IEnumerable<Type> GetKnownTypes(Type type, bool inherit)
        {
            IDictionary<Type, Type> knownTypes = new Dictionary<Type, Type>();
            IEnumerable<KnownTypeAttribute> knownTypeAttributes = type.GetCustomAttributes(typeof(KnownTypeAttribute), inherit).Cast<KnownTypeAttribute>();

            foreach (KnownTypeAttribute knownTypeAttribute in knownTypeAttributes)
            {
                Type knownType = knownTypeAttribute.Type;
                if (knownType != null)
                {
                    knownTypes[knownType] = knownType;
                }

                string methodName = knownTypeAttribute.MethodName;
                if (!String.IsNullOrEmpty(methodName))
                {
                    Type typeOfIEnumerableOfType = typeof(IEnumerable<Type>);
                    MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    if (methodInfo != null && typeOfIEnumerableOfType.IsAssignableFrom(methodInfo.ReturnType))
                    {
                        IEnumerable<Type> enumerable = methodInfo.Invoke(null, null) as IEnumerable<Type>;
                        if (enumerable != null)
                        {
                            foreach (Type t in enumerable)
                            {
                                knownTypes[t] = t;
                            }
                        }
                    }
                }
            }
            return knownTypes.Keys;
        }

        /// <summary>
        /// If the specified type is a generic Task, this function returns the
        /// inner task type.
        /// </summary>
        internal static Type UnwrapTaskInnerType(Type t)
        {
            if (typeof(Task).IsAssignableFrom(t) && t.IsGenericType)
            {
                return t.GetGenericArguments()[0];
            }

            return t;
        }
    }
}
