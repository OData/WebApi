//-----------------------------------------------------------------------------
// <copyright file="FastPropertyAccessor.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter.Deserialization;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// FastPropertyAccessor is a <see cref="PropertyAccessor{TStructuralType}"/> that speeds up (compares to reflection)
    /// a Getter and Setter for the PropertyInfo of TEntityType provided via the constructor.
    /// </summary>
    /// <typeparam name="TStructuralType">The type on which the PropertyInfo exists</typeparam>
    internal class FastPropertyAccessor<TStructuralType> : PropertyAccessor<TStructuralType> where TStructuralType : class
    {
        private bool _isCollection;
        private PropertyInfo _property;
        private Action<TStructuralType, object> _setter;
        private Func<object, object> _getter;

        public FastPropertyAccessor(PropertyInfo property)
            : base(property)
        {
            _property = property;
            _isCollection = TypeHelper.IsCollection(property.PropertyType);

            if (!_isCollection)
            {
                _setter = PropertyHelper.MakeFastPropertySetter<TStructuralType>(property);
            }
            _getter = PropertyHelper.MakeFastPropertyGetter(property);
        }

        public override object GetValue(TStructuralType instance)
        {
            if (instance == null)
            {
                throw Error.ArgumentNull("instance");
            }
            return _getter(instance);
        }

        public override void SetValue(TStructuralType instance, object value)
        {
            if (instance == null)
            {
                throw Error.ArgumentNull("instance");
            }

            if (_isCollection)
            {
                DeserializationHelpers.SetCollectionProperty(instance, _property.Name, edmPropertyType: null,
                    value: value, clearCollection: true);
            }
            else
            {
                _setter(instance, value);
            }
        }
    }
}
