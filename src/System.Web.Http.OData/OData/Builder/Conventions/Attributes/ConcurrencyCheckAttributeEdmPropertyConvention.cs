// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
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
        public override void Apply(PropertyConfiguration edmProperty,
            StructuralTypeConfiguration structuralTypeConfiguration,
            Attribute attribute)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            EntityTypeConfiguration entityType = structuralTypeConfiguration as EntityTypeConfiguration;
            PrimitivePropertyConfiguration primitiveProperty = edmProperty as PrimitivePropertyConfiguration;
            if (entityType != null && primitiveProperty != null)
            {
                if (!edmProperty.AddedExplicitly)
                {
                    primitiveProperty.ConcurrencyToken = true;
                }
            }
        }
    }
}
