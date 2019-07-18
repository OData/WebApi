// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Member descriptor
    /// </summary>
    public class MemberDescriptor
    {
        private readonly MemberInfo _memberInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberDescriptor"/> class.
        /// </summary>
        /// <param name="propertyInfo">Property information</param>
        public MemberDescriptor(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            this._memberInfo = propertyInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberDescriptor"/> class.
        /// </summary>
        /// <param name="methodInfo">Extension method information</param>
        public MemberDescriptor(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw Error.ArgumentNull("methodInfo");
            }

            this._memberInfo = methodInfo;
        }

        /// <summary>
        /// Obtains information about member
        /// </summary>
        public MemberInfo MemberInfo
        {
            get
            {
                return _memberInfo;
            }
        }

        /// <summary>
        /// Provide access to property metadata
        /// </summary>
        public PropertyInfo PropertyInfo
        {
            get
            {
                return _memberInfo as PropertyInfo;
            }
        }

        /// <summary>
        /// Provide access to extension property metadata
        /// </summary>
        public MethodInfo MethodInfo
        {
            get
            {
                return _memberInfo as MethodInfo;
            }
        }

        /// <summary>
        /// Get the name of the this property
        /// </summary>
        public string Name
        {
            get
            {
                return MemberInfo.Name;
            }
        }

        /// <summary>
        /// Get the type of the this property
        /// </summary>
        public Type PropertyType
        {
            get
            {
                if (PropertyInfo != null)
                {
                    return PropertyInfo.PropertyType;
                }

                return MethodInfo.ReturnType;
            }
        }

        /// <summary>
        /// Gets the class that declares this member
        /// </summary>
        public Type DeclaringType
        {
            get
            {
                if (PropertyInfo != null)
                {
                    return PropertyInfo.DeclaringType;
                }

                return MethodInfo.GetParameters()[0].ParameterType;
            }
        }

        /// <summary>
        /// Returns the reflected type from a member info.
        /// </summary>
        public Type ReflectedType
        {
            get
            {
                if (PropertyInfo != null)
                {
                    return TypeHelper.GetReflectedType(PropertyInfo);
                }

                return MethodInfo.GetParameters()[0].ParameterType;
            }
        }

        /// <summary>
        /// Cast MemberDescriptor to MemberInfo.
        /// </summary>
        /// <param name="memberDescriptor">The object to cast.</param>
        public static implicit operator MemberInfo(MemberDescriptor memberDescriptor)
        {
            return memberDescriptor.MemberInfo;
        }

        /// <summary>
        /// Returns a member information that represents the current object.
        /// </summary>
        /// <returns>A member information that represents the current object.</returns>
        public MemberInfo ToMemberInfo()
        {
            return MemberInfo;
        }

        /// <summary>
        /// Retrieves an array of the custom attributes applied to an assembly. A parameter specifies the assembly.
        /// </summary>
        /// <param name="type">The type of attribute to search for. Only attributes that are assignable to this type are returned. </param>
        /// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
        /// <returns>An array of custom attributes applied to this member, or an array with zero elements if no attributes assignable to attributeType have been applied.</returns>
        public object[] GetCustomAttributes(Type type, bool inherit)
        {
            return MemberInfo.GetCustomAttributes(type, inherit);
        }

        /// <summary>
        /// Retrieves a collection of custom attributes of a specified type that are applied to a specified member.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <returns>A collection of the custom attributes that are applied to element and that match T, or an empty collection if no such attributes exist. </returns>
        public IEnumerable<T> GetCustomAttributes<T>()
            where T : Attribute
        {
            return GetCustomAttributes<T>(false);
        }

        /// <summary>
        /// Retrieves a collection of custom attributes of a specified type that are applied to a specified member.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
        /// <returns>A collection of the custom attributes that are applied to element and that match T, or an empty collection if no such attributes exist. </returns>
        public IEnumerable<T> GetCustomAttributes<T>(bool inherit)
            where T : Attribute
        {
            return MemberInfo.GetCustomAttributes<T>(inherit);
        }

        /// <summary>
        /// Retrieves a custom attribute of a specified type that is applied to a specified member, and optionally inspects the ancestors of that member.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <returns>A custom attribute that matches T, or null if no such attribute is found.</returns>
        public T GetCustomAttribute<T>()
            where T : Attribute
        {
            return GetCustomAttribute<T>(false);
        }

        /// <summary>
        /// Retrieves a custom attribute of a specified type that is applied to a specified member, and optionally inspects the ancestors of that member.
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="inherit">true to inspect the ancestors of element; otherwise, false.</param>
        /// <returns>A custom attribute that matches T, or null if no such attribute is found.</returns>
        public T GetCustomAttribute<T>(bool inherit)
            where T : Attribute
        {
            return MemberInfo.GetCustomAttribute<T>(inherit);
        }

        /// <summary>
        /// Serves hash function. 
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return MemberInfo.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            MemberDescriptor propDescr = obj as MemberDescriptor;
            if (propDescr == null)
            {
                return false;
            }

            if (PropertyInfo != null)
            {
                return PropertyInfo.Equals(propDescr.PropertyInfo);
            }
            else
            {
                return MethodInfo.Equals(propDescr.MethodInfo);
            }
        }
    }
}
