// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
            if(methodInfo == null)
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
                    return PropertyInfo.PropertyType;
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
                    return PropertyInfo.DeclaringType;
                return MethodInfo.GetParameters()[0].ParameterType;
            }
        }

        /// <summary>
        /// Return the reflected type from a member info.
        /// </summary>
        public Type ReflectedType
        {
            get
            {
                if (PropertyInfo != null)
                    return TypeHelper.GetReflectedType(PropertyInfo);
                return MethodInfo.GetParameters()[0].ParameterType;
            }
        }

        /// <inheritdoc/>
        public static implicit operator MemberInfo(MemberDescriptor propertyDescriptor)
        {
            return propertyDescriptor.MemberInfo;
        }

        /// <inheritdoc/>
        public object[] GetCustomAttributes(Type type, bool inherit)
        {
            return MemberInfo.GetCustomAttributes(type, inherit);
        }

        /// <inheritdoc/>
        public IEnumerable<T> GetCustomAttributes<T>(bool inherit=false)
            where T:Attribute
        {
            return MemberInfo.GetCustomAttributes<T>(inherit);
        }

        /// <inheritdoc/>
        public T GetCustomAttribute<T>(bool inherit = false)
            where T : Attribute
        {
            return MemberInfo.GetCustomAttribute<T>(inherit);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return MemberInfo.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            MemberDescriptor propDescr = obj as MemberDescriptor;
            if (propDescr == null)
                return false;

            if (PropertyInfo != null)
                return PropertyInfo.Equals(propDescr.PropertyInfo);
            else
                return MethodInfo.Equals(propDescr.MethodInfo);
        }
    }
}
