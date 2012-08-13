// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    public class NavigationPropertyConfiguration : PropertyConfiguration
    {
        private readonly Type _relatedType = null;

        public NavigationPropertyConfiguration(PropertyInfo property, EdmMultiplicity multiplicity)
            : base(property)
        {
            if (property == null)
            {
                throw Error.ArgumentNull("property");
            }

            Multiplicity = multiplicity;

            _relatedType = property.PropertyType;
            if (multiplicity == EdmMultiplicity.Many)
            {
                //TODO: support use of a non-generic type i.e. public class OrdersCollection: List<Order>
                if (!_relatedType.IsGenericType || _relatedType.GetGenericArguments().Length > 1)
                {
                    throw Error.Argument("property", SRResources.ManyToManyNavigationPropertyMustReturnCollection);
                }

                _relatedType = _relatedType.GetGenericArguments()[0];
            }
        }

        public EdmMultiplicity Multiplicity { get; private set; }

        public override Type RelatedClrType
        {
            get { return this._relatedType; }
        }

        public override PropertyKind Kind
        {
            get { return PropertyKind.Navigation; }
        }
    }
}
