// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace System.Web.Http.OData.Builder.Conventions
{
    public class KeyAttributeConvention : AttributeEdmPropertyConvention<PrimitivePropertyConfiguration, KeyAttribute>
    {
        public override void Apply(PrimitivePropertyConfiguration edmProperty, IStructuralTypeConfiguration structuralTypeConfiguration, KeyAttribute attribute)
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
