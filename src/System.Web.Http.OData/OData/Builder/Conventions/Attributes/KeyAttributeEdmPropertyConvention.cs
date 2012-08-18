// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Configures properties that have the <see cref="KeyAttribute"/> as keys in the <see cref="IEdmEntityType"/>.
    /// </summary>
    public class KeyAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PrimitivePropertyConfiguration, KeyAttribute>
    {
        public KeyAttributeEdmPropertyConvention()
            : base(allowMultiple: false)
        {
        }

        /// <summary>
        /// Configures the property as a key on the edm type.
        /// </summary>
        /// <param name="edmProperty">The key property.</param>
        /// <param name="structuralTypeConfiguration">The edm type being configured.</param>
        /// <param name="attribute">The <see cref="KeyAttribute"/> found on the property.</param>
        public override void Apply(PrimitivePropertyConfiguration edmProperty, IStructuralTypeConfiguration structuralTypeConfiguration, KeyAttribute attribute)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            IEntityTypeConfiguration entity = structuralTypeConfiguration as IEntityTypeConfiguration;
            if (entity != null)
            {
                entity.HasKey(edmProperty.PropertyInfo);
            }
        }
    }
}
