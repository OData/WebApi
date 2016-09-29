// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Marks properties that have <see cref="ColumnAttribute"/> as the target EDM type.
    /// </summary>
    internal class ColumnAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public ColumnAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(ColumnAttribute), allowMultiple: false)
        {
        }

        /// <summary>
        /// Marks the property with the target EDM type.
        /// </summary>
        /// <param name="edmProperty">The EDM property.</param>
        /// <param name="structuralTypeConfiguration">The EDM type being configured.</param>
        /// <param name="attribute">The <see cref="Attribute"/> found.</param>
        /// <param name="model">The ODataConventionModelBuilder used to build the model.</param>
        public override void Apply(PropertyConfiguration edmProperty, StructuralTypeConfiguration structuralTypeConfiguration,
            Attribute attribute, ODataConventionModelBuilder model)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (edmProperty.AddedExplicitly)
            {
                return;
            }

            var primitiveProperty = edmProperty as PrimitivePropertyConfiguration;
            if (primitiveProperty == null)
            {
                return; // ignore non-primitive property
            }

            var columnAttribute = attribute as ColumnAttribute;
            if (columnAttribute == null || columnAttribute.TypeName == null)
            {
                return; // ignore the column type
            }

            string typeName = columnAttribute.TypeName;
            if (String.Compare(typeName, "date", StringComparison.OrdinalIgnoreCase) == 0)
            {
                primitiveProperty.AsDate();
            }
            else if (String.Compare(typeName, "time", StringComparison.OrdinalIgnoreCase) == 0)
            {
                primitiveProperty.AsTimeOfDay();
            }
        }
    }
}
