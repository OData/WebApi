// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Configures properties that have the <see cref="KeyAttribute"/> as keys in the <see cref="IEdmEntityType"/>.
    /// </summary>
    internal class KeyAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<StructuralPropertyConfiguration>
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
        /// <param name="model">The ODataConventionModelBuilder used to build the model.</param>
        public override void Apply(StructuralPropertyConfiguration edmProperty,
            StructuralTypeConfiguration structuralTypeConfiguration,
            Attribute attribute,
            ODataConventionModelBuilder model)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (edmProperty.Kind == PropertyKind.Primitive || edmProperty.Kind == PropertyKind.Enum)
            {
                EntityTypeConfiguration entity = structuralTypeConfiguration as EntityTypeConfiguration;
                if (entity != null)
                {
                    entity.HasKey(edmProperty.PropertyInfo);
                }
            }
        }
    }
}
