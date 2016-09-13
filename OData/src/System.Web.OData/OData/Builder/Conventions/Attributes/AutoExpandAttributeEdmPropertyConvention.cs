// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;

namespace System.Web.OData.Builder.Conventions.Attributes
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
