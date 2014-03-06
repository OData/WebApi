// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Web.Http;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Marks properties that have <see cref="ConcurrencyCheckAttribute"/> as non-optional on their EDM type.
    /// </summary>
    internal class ConcurrencyCheckAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public ConcurrencyCheckAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(ConcurrencyCheckAttribute), allowMultiple: false)
        {
        }

        /// <summary>
        /// Marks the property with concurrency token on the EDM type.
        /// </summary>
        /// <param name="edmProperty">The EDM property.</param>
        /// <param name="structuralTypeConfiguration">The EDM type being configured.</param>
        /// <param name="attribute">The <see cref="Attribute"/> found.</param>
        /// <param name="model">The ODataConventionModelBuilder used to build the model.</param>
        public override void Apply(PropertyConfiguration edmProperty,
            StructuralTypeConfiguration structuralTypeConfiguration,
            Attribute attribute,
            ODataConventionModelBuilder model)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            EntityTypeConfiguration entityType = structuralTypeConfiguration as EntityTypeConfiguration;
            PrimitivePropertyConfiguration primitiveProperty = edmProperty as PrimitivePropertyConfiguration;
            if (entityType != null && primitiveProperty != null)
            {
                primitiveProperty.ConcurrencyToken = true;
            }
        }
    }
}
