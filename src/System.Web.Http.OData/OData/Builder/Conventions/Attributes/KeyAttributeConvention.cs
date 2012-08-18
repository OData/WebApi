// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Configures properties that have the <see cref="KeyAttribute"/> as keys in the <see cref="IEdmEntityType"/>.
    /// </summary>
    public class KeyAttributeConvention : AttributeEdmPropertyConvention<PrimitivePropertyConfiguration, KeyAttribute>
    {
        public KeyAttributeConvention()
            : base(allowMultiple: false)
        {
        }

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
