// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Represents the configuration for a navigation property of an entity type.
    /// </summary>
    /// <remarks>This configuration functionality is exposed by the model builder Fluent API, see <see cref="ODataModelBuilder"/>.</remarks>
    public class NavigationPropertyConfiguration : PropertyConfiguration
    {
        private readonly Type _relatedType = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPropertyConfiguration"/> class.
        /// </summary>
        /// <param name="property">The backing CLR property.</param>
        /// <param name="multiplicity">The <see cref="EdmMultiplicity"/>.</param>
        /// <param name="declaringType">The declaring entity type.</param>
        public NavigationPropertyConfiguration(PropertyInfo property, EdmMultiplicity multiplicity, EntityTypeConfiguration declaringType)
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
                    throw Error.Argument("property", SRResources.ManyToManyNavigationPropertyMustReturnCollection, property.Name, property.ReflectedType.Name);
                }

                _relatedType = elementType;
            }
        }

        /// <summary>
        /// Gets the declaring entity type.
        /// </summary>
        public EntityTypeConfiguration DeclaringEntityType
        {
            get
            {
                return DeclaringType as EntityTypeConfiguration;
            }
        }

        /// <summary>
        /// Gets the <see cref="EdmMultiplicity"/> of this navigation property.
        /// </summary>
        public EdmMultiplicity Multiplicity { get; private set; }

        /// <summary>
        /// Gets the backing CLR type of this property type.
        /// </summary>
        public override Type RelatedClrType
        {
            get { return _relatedType; }
        }

        /// <summary>
        /// Gets the <see cref="PropertyKind"/> of this property.
        /// </summary>
        public override PropertyKind Kind
        {
            get { return PropertyKind.Navigation; }
        }

        /// <summary>
        /// Marks the navigation property as optional.
        /// </summary>
        public NavigationPropertyConfiguration Optional()
        {
            if (Multiplicity == EdmMultiplicity.Many)
            {
                throw Error.InvalidOperation(SRResources.ManyNavigationPropertiesCannotBeChanged, Name);
            }

            Multiplicity = EdmMultiplicity.ZeroOrOne;
            return this;
        }

        /// <summary>
        /// Marks the navigation property as required.
        /// </summary>
        public NavigationPropertyConfiguration Required()
        {
            if (Multiplicity == EdmMultiplicity.Many)
            {
                throw Error.InvalidOperation(SRResources.ManyNavigationPropertiesCannotBeChanged, Name);
            }

            Multiplicity = EdmMultiplicity.One;
            return this;
        }
    }
}
