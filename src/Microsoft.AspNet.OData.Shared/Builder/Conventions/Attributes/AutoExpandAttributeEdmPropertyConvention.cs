//-----------------------------------------------------------------------------
// <copyright file="AutoExpandAttributeEdmPropertyConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class AutoExpandAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<NavigationPropertyConfiguration>
    {
        public AutoExpandAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(AutoExpandAttribute), allowMultiple: false)
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
                AutoExpandAttribute autoExpandAttribute = attribute as AutoExpandAttribute;
                edmProperty.AutomaticallyExpand(autoExpandAttribute.DisableWhenSelectPresent);
            }
        }
    }
}
