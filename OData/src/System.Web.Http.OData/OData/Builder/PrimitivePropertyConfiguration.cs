// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Web.Http.OData.Builder
{
    /// <summary>
    /// Used to configure a primitive property of an entity type or complex type.
    /// This configuration functionality is exposed by the model builder Fluent API, see <see cref="ODataModelBuilder"/>.
    /// </summary>
    public class PrimitivePropertyConfiguration : StructuralPropertyConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitivePropertyConfiguration"/> class.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="declaringType">The declaring EDM type of the property.</param>
        public PrimitivePropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
            : base(property, declaringType)
        {
        }

        /// <summary>
        /// Gets the type of this property.
        /// </summary>
        public override PropertyKind Kind
        {
            get { return PropertyKind.Primitive; }
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
        public PrimitivePropertyConfiguration IsOptional()
        {
            OptionalProperty = true;
            return this;
        }

        /// <summary>
        /// Configures the property to be required.
        /// </summary>
        /// <returns>Returns itself so that multiple calls can be chained.</returns>
        public PrimitivePropertyConfiguration IsRequired()
        {
            OptionalProperty = false;
            return this;
        }
    }
}
