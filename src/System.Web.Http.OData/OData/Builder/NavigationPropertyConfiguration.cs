// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Http.OData.Builder.Conventions;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    public class NavigationPropertyConfiguration : PropertyConfiguration
    {
        private readonly Type _relatedType = null;

        public NavigationPropertyConfiguration(PropertyInfo property, EdmMultiplicity multiplicity, IEntityTypeConfiguration declaringType)
            : base(property, declaringType)
        {
            if (property == null)
            {
                throw Error.ArgumentNull("property");
            }

            Multiplicity = multiplicity;

            _relatedType = property.PropertyType;
            if (multiplicity == EdmMultiplicity.Many)
            {
                Type elementType;
                if (!_relatedType.IsCollection(out elementType))
                {
                    throw Error.InvalidOperation(SRResources.ManyToManyNavigationPropertyMustReturnCollection, property.Name, property.ReflectedType.Name);
                }

                _relatedType = elementType;
            }
        }

        public IEntityTypeConfiguration DeclaringEntityType
        {
            get
            {
                return DeclaringType as IEntityTypeConfiguration;
            }
        }

        public EdmMultiplicity Multiplicity { get; private set; }

        public override Type RelatedClrType
        {
            get { return _relatedType; }
        }

        public override PropertyKind Kind
        {
            get { return PropertyKind.Navigation; }
        }
    }
}
