//-----------------------------------------------------------------------------
// <copyright file="CollectionPropertyConfiguration.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// CollectionPropertyConfiguration represents a CollectionProperty on either an EntityType or ComplexType.
    /// </summary>
    public class CollectionPropertyConfiguration : StructuralPropertyConfiguration
    {
        private Type _elementType;

        /// <summary>
        /// Constructs a CollectionPropertyConfiguration using the <paramref name="property">property</paramref> provided.
        /// </summary>
        public CollectionPropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
            : base(property, declaringType)
        {
            if (!TypeHelper.IsCollection(property.PropertyType, out _elementType))
            {
                throw Error.Argument("property", SRResources.CollectionPropertiesMustReturnIEnumerable, property.Name, property.DeclaringType.FullName);
            }
        }

        /// <inheritdoc />
        public override PropertyKind Kind
        {
            get { return PropertyKind.Collection; }
        }

        /// <inheritdoc />
        public override Type RelatedClrType
        {
            get { return ElementType; }
        }

        /// <summary>
        /// Returns the type of Elements in the Collection
        /// </summary>
        public Type ElementType
        {
            get { return _elementType; }
        }

        /// <summary>
        /// Sets the CollectionProperty to optional (i.e. nullable).
        /// </summary>
        public CollectionPropertyConfiguration IsOptional()
        {
            OptionalProperty = true;
            return this;
        }

        /// <summary>
        /// Sets the CollectionProperty to required (i.e. non-nullable).
        /// </summary>
        public CollectionPropertyConfiguration IsRequired()
        {
            OptionalProperty = false;
            return this;
        }

        /// <summary>
        /// Adds subtypes to the list of derived type constraints.
        /// </summary>
        /// <param name="subtypes">The subtypes for which the constraint needs to be added.</param>
        /// <returns>Updated configuration object.</returns>
        public CollectionPropertyConfiguration HasDerivedTypeConstraints(params Type[] subtypes)
        {
            DerivedTypeConstraints.AddConstraints(subtypes);
            return this;
        }

        /// <summary>
        /// Adds TDerived to the list of derived type constraints.
        /// </summary>
        /// <returns>Updated configuration object.</returns>
        public CollectionPropertyConfiguration HasDerivedTypeConstraint<TDerivedType>()
        {
            DerivedTypeConstraints.AddConstraint<TDerivedType>();
            return this;
        }
    }
}
