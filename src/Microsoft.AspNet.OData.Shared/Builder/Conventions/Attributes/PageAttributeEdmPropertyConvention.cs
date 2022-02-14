//-----------------------------------------------------------------------------
// <copyright file="PageAttributeEdmPropertyConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class PageAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public PageAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(PageAttribute), allowMultiple: false)
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
                PageAttribute pageAttribute = attribute as PageAttribute;
                ModelBoundQuerySettings querySettings = edmProperty.QueryConfiguration.GetModelBoundQuerySettingsOrDefault();
                if (pageAttribute.MaxTop < 0)
                {
                    querySettings.MaxTop = null;
                }
                else
                {
                    querySettings.MaxTop = pageAttribute.MaxTop;
                }

                if (pageAttribute.PageSize > 0)
                {
                    querySettings.PageSize = pageAttribute.PageSize;
                }
            }
        }
    }
}
