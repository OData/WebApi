// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Configures properties that have the <see cref="KeyAttribute"/> as keys in the <see cref="IEdmEntityType"/>.
    /// </summary>
    internal class KeyAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PrimitivePropertyConfiguration>
    {
        public KeyAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(KeyAttribute), allowMultiple: false)
        {
        }

        /// <summary>
        /// Configures the property as a key on the edm type.
        /// </summary>
        /// <param name="edmProperty">The key property.</param>
        /// <param name="structuralTypeConfiguration">The edm type being configured.</param>
        /// <param name="attribute">The <see cref="Attribute"/> found on the property.</param>
        public override void Apply(PrimitivePropertyConfiguration edmProperty, StructuralTypeConfiguration structuralTypeConfiguration, Attribute attribute)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            EntityTypeConfiguration entity = structuralTypeConfiguration as EntityTypeConfiguration;
            if (entity != null)
            {
                entity.HasKey(edmProperty.PropertyInfo);
            }
        }
    }
}
