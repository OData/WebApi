//-----------------------------------------------------------------------------
// <copyright file="ExpandAttributeEdmPropertyConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class ExpandAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public ExpandAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(ExpandAttribute), allowMultiple: true)
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
                ExpandAttribute expandAttribute = attribute as ExpandAttribute;
                ModelBoundQuerySettings querySettings = edmProperty.QueryConfiguration.GetModelBoundQuerySettingsOrDefault();
                if (querySettings.ExpandConfigurations.Count == 0)
                {
                    querySettings.CopyExpandConfigurations(expandAttribute.ExpandConfigurations);
                }
                else
                {
                    foreach (var property in expandAttribute.ExpandConfigurations.Keys)
                    {
                        querySettings.ExpandConfigurations[property] =
                            expandAttribute.ExpandConfigurations[property];
                    }
                }

                if (expandAttribute.ExpandConfigurations.Count == 0)
                {
                    querySettings.DefaultExpandType = expandAttribute.DefaultExpandType;
                    querySettings.DefaultMaxDepth = expandAttribute.DefaultMaxDepth ?? ODataValidationSettings.DefaultMaxExpansionDepth;
                }
            }
        }
    }
}
