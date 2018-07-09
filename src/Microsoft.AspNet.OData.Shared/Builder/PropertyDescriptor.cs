using Microsoft.AspNet.OData.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Property descriptor
    /// </summary>
    public class PropertyDescriptor
    {
        private readonly PropertyInfo _propertyInfo;
        private readonly MethodInfo _methodInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDescriptor"/> class.
        /// </summary>
        /// <param name="propertyInfo">Property information</param>
        public PropertyDescriptor(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw Error.ArgumentNull("propertyInfo");
            }

            this._propertyInfo = propertyInfo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDescriptor"/> class.
        /// </summary>
        /// <param name="methodInfo">Extension property information</param>
        public PropertyDescriptor(MethodInfo methodInfo)
        {
            if(methodInfo==null)
            {
                throw Error.ArgumentNull("methodInfo");
            }
            this._methodInfo = methodInfo;
        }

        /// <summary>
        /// Obtains information about member
        /// </summary>
        public MemberInfo MemberInfo
        {
            get
            {
                if (_propertyInfo != null)
                    return _propertyInfo;
                return _methodInfo;
            }
        }

        /// <summary>
        /// Provide access to property metadata
        /// </summary>
        public PropertyInfo PropertyInfo
        {
            get
            {
                return _propertyInfo;
            }
        }

        /// <summary>
        /// Provide access to extension property metadata
        /// </summary>
        public MethodInfo MethodInfo
        {
            get
            {
                return _methodInfo;
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
                if (_propertyInfo != null)
                    return _propertyInfo.PropertyType;
                return _methodInfo.ReturnType;
            }
        }

        /// <summary>
        /// Gets the class that declares this member
        /// </summary>
        public Type DeclaringType
        {
            get
            {
                if (_propertyInfo != null)
                    return _propertyInfo.DeclaringType;
                return _methodInfo.GetParameters()[0].ParameterType;
            }
        }

        /// <summary>
        /// Return the reflected type from a member info.
        /// </summary>
        public Type ReflectedType
        {
            get
            {
                if (_propertyInfo != null)
                    return TypeHelper.GetReflectedType(_propertyInfo);
                return _methodInfo.GetParameters()[0].ParameterType;
            }
        }

        /// <inheritdoc/>
        public static implicit operator MemberInfo(PropertyDescriptor propertyDescriptor)
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
            PropertyDescriptor propDescr = obj as PropertyDescriptor;
            if (propDescr == null)
                return false;

            if (PropertyInfo != null)
                return PropertyInfo.Equals(propDescr.PropertyInfo);
            else
                return MethodInfo.Equals(propDescr.MethodInfo);
        }
    }
}
