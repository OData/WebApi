// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Http.OData.Builder
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
    }
}
