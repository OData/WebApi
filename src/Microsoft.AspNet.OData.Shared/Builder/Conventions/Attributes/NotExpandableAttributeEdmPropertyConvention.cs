//-----------------------------------------------------------------------------
// <copyright file="NotExpandableAttributeEdmPropertyConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class NotExpandableAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<NavigationPropertyConfiguration>
    {
        public NotExpandableAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(NotExpandableAttribute), allowMultiple: false)
        {
        }

        public override void Apply(NavigationPropertyConfiguration edmProperty,
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
                edmProperty.IsNotExpandable();
            }
        }
    }
}
