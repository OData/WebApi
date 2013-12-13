// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using WebApiResources = System.Web.Http.OData.Properties.SRResources;

namespace System.Web.Http.OData
{
    /// <summary>
    /// Represents a strategy for Getting and Setting a PropertyInfo on <typeparamref name="TEntityType"/>
    /// </summary>
    /// <typeparam name="TEntityType">The type that contains the PropertyInfo</typeparam>
    internal abstract class PropertyAccessor<TEntityType> where TEntityType : class
    {
        protected PropertyAccessor(PropertyInfo property)
        {
            if (property == null)
            {
                throw Error.ArgumentNull("property");
            }
            Property = property;
            if (Property.GetGetMethod() == null ||
                (!Property.PropertyType.IsCollection() && Property.GetSetMethod() == null))
            {
                throw Error.Argument("property", WebApiResources.PropertyMustHavePublicGetterAndSetter);
            }
        }

        public PropertyInfo Property
        {
            get;
            private set;
        }

        public void Copy(TEntityType from, TEntityType to)
        {
            if (from == null)
            {
                throw Error.ArgumentNull("from");
            }
            if (to == null)
            {
                throw Error.ArgumentNull("to");
            }
            SetValue(to, GetValue(from));
        }

        public abstract object GetValue(TEntityType entity);

        public abstract void SetValue(TEntityType entity, object value);
    }
}
