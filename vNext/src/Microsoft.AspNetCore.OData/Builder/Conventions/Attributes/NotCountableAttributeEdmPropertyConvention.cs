// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Query;

namespace Microsoft.AspNetCore.OData.Builder.Conventions.Attributes
{
    internal class NotCountableAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public NotCountableAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(NotCountableAttribute), allowMultiple: false)
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
                edmProperty.IsNotCountable();
            }
        }
    }
}