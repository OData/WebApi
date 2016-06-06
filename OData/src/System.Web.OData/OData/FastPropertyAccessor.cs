// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Http;
using System.Web.Http.Internal;
using System.Web.OData.Formatter.Deserialization;

namespace System.Web.OData
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
            _isCollection = property.PropertyType.IsCollection();

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
