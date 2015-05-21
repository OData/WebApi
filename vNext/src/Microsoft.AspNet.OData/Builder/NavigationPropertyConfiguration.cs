// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using System.Web.OData.Formatter;

using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Represents the configuration for a navigation property of an entity type.
    /// </summary>
    /// <remarks>This configuration functionality is exposed by the model builder Fluent API, see <see cref="ODataModelBuilder"/>.</remarks>
    public class NavigationPropertyConfiguration : PropertyConfiguration
    {
        private readonly Type _relatedType;
        private readonly IDictionary<PropertyInfo, PropertyInfo> _referentialConstraint =
            new Dictionary<PropertyInfo, PropertyInfo>();

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
                    //throw Error.Argument("property", SRResources.ManyToManyNavigationPropertyMustReturnCollection, property.Name, property.ReflectedType.Name);
                }

                _relatedType = elementType;
            }

            OnDeleteAction = EdmOnDeleteAction.None;
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
        /// Gets whether this navigation property is a containment, default to false.
        /// </summary>
        public bool ContainsTarget { get; private set; }

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
        /// Gets or sets the delete action for this navigation property.
        /// </summary>
        public EdmOnDeleteAction OnDeleteAction { get; set; }

        /// <summary>
        /// Gets the foreign keys in the referential constraint of this navigation property.
        /// </summary>
        public IEnumerable<PropertyInfo> DependentProperties
        {
            get { return _referentialConstraint.Keys; }
        }

        /// <summary>
        /// Gets the target keys in the referential constraint of this navigation property.
        /// </summary>
        public IEnumerable<PropertyInfo> PrincipalProperties
        {
            get { return _referentialConstraint.Values; }
        }

        /// <summary>
        /// Marks the navigation property as optional.
        /// </summary>
        public NavigationPropertyConfiguration Optional()
        {
            //if (Multiplicity == EdmMultiplicity.Many)
            //{
            //    throw Error.InvalidOperation(SRResources.ManyNavigationPropertiesCannotBeChanged, Name);
            //}

            Multiplicity = EdmMultiplicity.ZeroOrOne;
            return this;
        }

        /// <summary>
        /// Marks the navigation property as required.
        /// </summary>
        public NavigationPropertyConfiguration Required()
        {
            //if (Multiplicity == EdmMultiplicity.Many)
            //{
            //    throw Error.InvalidOperation(SRResources.ManyNavigationPropertiesCannotBeChanged, Name);
            //}

            Multiplicity = EdmMultiplicity.One;
            return this;
        }

        /// <summary>
        /// Marks the navigation property as containment.
        /// </summary>
        public NavigationPropertyConfiguration Contained()
        {
            ContainsTarget = true;
            return this;
        }

        /// <summary>
        /// Marks the navigation property as non-contained.
        /// </summary>
        public NavigationPropertyConfiguration NonContained()
        {
            ContainsTarget = false;
            return this;
        }

        /// <summary>
        /// Configures cascade delete to be on for the navigation property.
        /// </summary>
        public NavigationPropertyConfiguration CascadeOnDelete()
        {
            CascadeOnDelete(cascade: true);
            return this;
        }

        /// <summary>
        /// Configures whether or not cascade delete is on for the navigation property.
        /// </summary>
        /// <param name="cascade"><c>true</c> indicates delete should also remove the associated items;
        /// <c>false</c> indicates no additional action on delete.</param>
        public NavigationPropertyConfiguration CascadeOnDelete(bool cascade)
        {
            OnDeleteAction = cascade ? EdmOnDeleteAction.Cascade : EdmOnDeleteAction.None;
            return this;
        }

        /// <summary>
        /// Configures the referential constraint with the specified <parameref name="dependentPropertyInfo"/>
        /// and <parameref name="principalPropertyInfo" />.
        /// </summary>
        /// <param name="dependentPropertyInfo">The dependent property info for the referential constraint.</param>
        /// <param name="principalPropertyInfo">The principal property info for the referential constraint.</param>
        public NavigationPropertyConfiguration HasConstraint(PropertyInfo dependentPropertyInfo,
            PropertyInfo principalPropertyInfo)
        {
            return HasConstraint(new KeyValuePair<PropertyInfo, PropertyInfo>(dependentPropertyInfo,
                principalPropertyInfo));
        }

        /// <summary>
        /// Configures the referential constraint with the dependent and principal property pair.
        /// </summary>
        /// <param name="constraint">The dependent and principal property pair.</param>
        public NavigationPropertyConfiguration HasConstraint(KeyValuePair<PropertyInfo, PropertyInfo> constraint)
        {
            if (constraint.Key == null)
            {
                throw Error.ArgumentNull("dependentPropertyInfo");
            }

            if (constraint.Value == null)
            {
                throw Error.ArgumentNull("principalPropertyInfo");
            }

            //if (Multiplicity == EdmMultiplicity.Many)
            //{
            //    throw Error.NotSupported(SRResources.ReferentialConstraintOnManyNavigationPropertyNotSupported,
            //        Name, DeclaringEntityType.ClrType.FullName);
            //}

            if (ValidateConstraint(constraint))
            {
                return this;
            }

            EntityTypeConfiguration principalEntity = DeclaringEntityType.ModelBuilder.StructuralTypes
                    .OfType<EntityTypeConfiguration>().FirstOrDefault(e => e.ClrType == RelatedClrType);
            Contract.Assert(principalEntity != null);

            PrimitivePropertyConfiguration principal = principalEntity.AddProperty(constraint.Value);
            PrimitivePropertyConfiguration dependent = DeclaringEntityType.AddProperty(constraint.Key);

            // If the navigation property on which the referential constraint is defined or the principal property
            // is nullable, then the dependent property MUST be nullable.
            if (Multiplicity == EdmMultiplicity.ZeroOrOne || principal.OptionalProperty)
            {
                dependent.OptionalProperty = true;
            }

            // If both the navigation property and the principal property are not nullable,
            // then the dependent property MUST be marked with the Nullable="false" attribute value.
            if (Multiplicity == EdmMultiplicity.One && !principal.OptionalProperty)
            {
                dependent.OptionalProperty = false;
            }

            _referentialConstraint.Add(constraint);
            return this;
        }

        private bool ValidateConstraint(KeyValuePair<PropertyInfo, PropertyInfo> constraint)
        {
            if (_referentialConstraint.Contains(constraint))
            {
                return true;
            }

            PropertyInfo value;
            if (_referentialConstraint.TryGetValue(constraint.Key, out value))
            {
                //throw Error.InvalidOperation(SRResources.ReferentialConstraintAlreadyConfigured, "dependent",
                //    constraint.Key.Name, "principal", value.Name);
            }

            if (PrincipalProperties.Any(p => p == constraint.Value))
            {
                PropertyInfo foundDependent = _referentialConstraint.First(r => r.Value == constraint.Value).Key;

                //throw Error.InvalidOperation(SRResources.ReferentialConstraintAlreadyConfigured, "principal",
                //    constraint.Value.Name, "dependent", foundDependent.Name);
            }

            // The principal property and the dependent property must have the same data type.
            //if (constraint.Key.PropertyType != constraint.Value.PropertyType)
            //{
            //    throw Error.InvalidOperation(SRResources.DependentAndPrincipalTypeNotMatch,
            //        constraint.Key.PropertyType.FullName, constraint.Value.PropertyType.FullName);
            //}

            // OData V4 spec says that the principal and dependent property MUST be a path expression resolving to a primitive
            // property of the dependent entity type itself or to a primitive property of a complex property (recursively) of
            // the dependent entity type.
            // So far, ODL doesn't support to allow a primitive property of a complex property to be the dependent/principal property.
            // There's an issue tracking on: https://github.com/OData/odata.net/issues/22
            if (EdmLibHelpers.GetEdmPrimitiveTypeOrNull(constraint.Key.PropertyType) == null)
            {
                //throw Error.InvalidOperation(SRResources.ReferentialConstraintPropertyTypeNotValid,
                //    constraint.Key.PropertyType.FullName);
            }

            return false;
        }
    }
}
