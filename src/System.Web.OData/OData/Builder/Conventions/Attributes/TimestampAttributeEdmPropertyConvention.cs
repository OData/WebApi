// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    internal class TimestampAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public TimestampAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(TimestampAttribute), allowMultiple: false)
        {
        }

        public override void Apply(PropertyConfiguration edmProperty,
            StructuralTypeConfiguration structuralTypeConfiguration,
            Attribute attribute,
            ODataConventionModelBuilder model)
        {
            EntityTypeConfiguration entity = structuralTypeConfiguration as EntityTypeConfiguration;
            if (entity != null)
            {
                PrimitivePropertyConfiguration[] timestampProperties = GetPropertiesWithTimestamp(entity);

                // We only support one Timestamp column per type, as a SQL table (the underlying concept this attribute
                // is bounded to) only supports one row version column per table.
                if (timestampProperties.Length == 1)
                {
                    timestampProperties[0].IsConcurrencyToken();
                }
            }
        }

        private static PrimitivePropertyConfiguration[] GetPropertiesWithTimestamp(EntityTypeConfiguration config)
        {
            IEnumerable<PropertyConfiguration> properties = config.ThisAndBaseTypes().SelectMany(p => p.Properties);
            return properties.OfType<PrimitivePropertyConfiguration>()
                .Where(pc => pc.PropertyInfo.GetCustomAttributes(typeof(TimestampAttribute), inherit: true).Length > 0)
                .ToArray();
        }
    }
}
