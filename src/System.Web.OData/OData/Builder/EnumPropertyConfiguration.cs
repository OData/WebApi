// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Used to configure an enum property of an entity type or complex type.
    /// This configuration functionality is exposed by the model builder Fluent API, see <see cref="ODataModelBuilder"/>.
    /// </summary>
    public class EnumPropertyConfiguration : StructuralPropertyConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumPropertyConfiguration"/> class.
        /// </summary>
        /// <param name="property">The property of the configuration.</param>
        /// <param name="declaringType">The declaring type of the property.</param>
        public EnumPropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
            : base(property, declaringType)
        {
        }

        /// <summary>
        /// Gets the type of this property.
        /// </summary>
        public override PropertyKind Kind
        {
            get { return PropertyKind.Enum; }
        }

        /// <summary>
        /// Gets the backing CLR type of this property type.
        /// </summary>
        public override Type RelatedClrType
        {
            get { return PropertyInfo.PropertyType; }
        }

        /// <summary>
        /// Configures the property to be optional.
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public EnumPropertyConfiguration IsOptional()
        {
            OptionalProperty = true;
            return this;
        }

        /// <summary>
        /// Configures the property to be required.
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public EnumPropertyConfiguration IsRequired()
        {
            OptionalProperty = false;
            return this;
        }

        /// <summary>
        /// Configures the property to be used in concurrency checks. For OData this means to be part of the ETag.
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public EnumPropertyConfiguration IsConcurrencyToken()
        {
            ConcurrencyToken = true;
            return this;
        }
    }
}
