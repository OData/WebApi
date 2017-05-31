// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class UnsortableAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public UnsortableAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(UnsortableAttribute), allowMultiple: false)
        {
        }

        public override void Apply(PropertyConfiguration edmProperty, 
            StructuralTypeConfiguration structuralTypeConfiguration, 
            Attribute attribute,
            ODataConventionModelBuilder model)
        {
            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (!edmProperty.AddedExplicitly)
            {
                edmProperty.IsNotSortable();
            }
        }
    }
}
