//-----------------------------------------------------------------------------
// <copyright file="PropertyAccessor.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
                (!TypeHelper.IsCollection(property.PropertyType) && Property.GetSetMethod() == null))
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
