// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents a strategy for Getting and Setting a PropertyInfo on <typeparamref name="TStructuralType"/>
    /// </summary>
    /// <typeparam name="TStructuralType">The type that contains the PropertyInfo</typeparam>
    internal abstract class PropertyAccessor<TStructuralType> where TStructuralType : class
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
                throw Error.Argument("property", SRResources.PropertyMustHavePublicGetterAndSetter);
            }
        }

        public PropertyInfo Property
        {
            get;
            private set;
        }

        public void Copy(TStructuralType from, TStructuralType to)
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

        public abstract object GetValue(TStructuralType instance);

        public abstract void SetValue(TStructuralType instance, object value);
    }
}
