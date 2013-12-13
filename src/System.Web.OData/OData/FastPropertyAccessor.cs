// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Http.Internal;
using System.Web.Http.OData.Formatter.Deserialization;

namespace System.Web.Http.OData
{
    /// <summary>
    /// FastPropertyAccessor is a <see cref="PropertyAccessor{TEntityType}"/> that speeds up (compares to reflection)
    /// a Getter and Setter for the PropertyInfo of TEntityType provided via the constructor.
    /// </summary>
    /// <typeparam name="TEntityType">The type on which the PropertyInfo exists</typeparam>
    internal class FastPropertyAccessor<TEntityType> : PropertyAccessor<TEntityType> where TEntityType : class
    {
        private bool _isCollection;
        private PropertyInfo _property;
        private Action<TEntityType, object> _setter;
        private Func<object, object> _getter;

        public FastPropertyAccessor(PropertyInfo property)
            : base(property)
        {
            _property = property;
            _isCollection = property.PropertyType.IsCollection();

            if (!_isCollection)
            {
                _setter = PropertyHelper.MakeFastPropertySetter<TEntityType>(property);
            }
            _getter = PropertyHelper.MakeFastPropertyGetter(property);
        }

        public override object GetValue(TEntityType entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            return _getter(entity);
        }

        public override void SetValue(TEntityType entity, object value)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            if (_isCollection)
            {
                DeserializationHelpers.SetCollectionProperty(entity, _property.Name, edmPropertyType: null,
                    value: value, clearCollection: true);
            }
            else
            {
                _setter(entity, value);
            }
        }
    }
}
