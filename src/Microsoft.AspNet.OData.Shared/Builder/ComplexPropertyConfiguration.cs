// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Represents the configuration for a complex property of a structural type (an entity type or a complex type).
    /// </summary>
    public class ComplexPropertyConfiguration : StructuralPropertyConfiguration
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="ComplexPropertyConfiguration"/> class.
        /// </summary>
        /// <param name="property">The property of the configuration.</param>
        /// <param name="declaringType">The declaring type of the property.</param>
        public ComplexPropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
            : base(property, declaringType)
        {
        }

        /// <inheritdoc />
        public override PropertyKind Kind
        {
            get { return PropertyKind.Complex; }
        }

        /// <inheritdoc />
        public override Type RelatedClrType
        {
            get { return PropertyInfo.PropertyType; }
        }

        /// <summary>
        /// Marks the current complex property as optional.
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public ComplexPropertyConfiguration IsOptional()
        {
            OptionalProperty = true;
            return this;
        }

        /// <summary>
        /// Marks the current complex property as required.
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public ComplexPropertyConfiguration IsRequired()
        {
            OptionalProperty = false;
            return this;
        }

        /// <summary>
        /// Adds subtypes to the list of derived type constraints.
        /// </summary>
        /// <param name="subtypes">The subtypes for which the constraint needs to be added.</param>
        /// <returns>Updated configuration object.</returns>
        public ComplexPropertyConfiguration AddDerivedTypeConstraint(params Type[] subtypes)
        {
            AddDerivedTypeConstraintImpl(subtypes);
            return this;
        }
    }
}