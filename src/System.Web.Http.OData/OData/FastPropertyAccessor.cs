// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Reflection;
using System.Web.Http.Internal;

namespace System.Web.Http.OData
{
    /// <summary>
    /// FastPropertyAccessor is a <see cref="PropertyAccessor{TEntityType}"/> that speeds up (compares to reflection)
    /// a Getter and Setter for the PropertyInfo of TEntityType provided via the constructor.
    /// </summary>
    /// <typeparam name="TEntityType">The type on which the PropertyInfo exists</typeparam>
    internal class FastPropertyAccessor<TEntityType> : PropertyAccessor<TEntityType> where TEntityType : class
    {
        private Action<TEntityType, object> _setter;
        private Func<object, object> _getter;

        public FastPropertyAccessor(PropertyInfo property)
            : base(property)
        {
            _setter = PropertyHelper.MakeFastPropertySetter<TEntityType>(property);
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
            _setter(entity, value);
        }
    }
}
