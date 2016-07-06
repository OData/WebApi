// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Query;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    internal class FilterAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public FilterAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(FilterAttribute), allowMultiple: true)
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
                FilterAttribute filterAttribute = attribute as FilterAttribute;
                ModelBoundQuerySettings querySettings = edmProperty.QueryConfiguration.GetModelBoundQuerySettingsOrDefault();
                if (querySettings.FilterConfigurations.Count == 0)
                {
                    querySettings.CopyFilterConfigurations(filterAttribute.FilterConfigurations);
                }
                else
                {
                    foreach (var property in filterAttribute.FilterConfigurations.Keys)
                    {
                        querySettings.FilterConfigurations[property] =
                            filterAttribute.FilterConfigurations[property];
                    }
                }

                if (filterAttribute.FilterConfigurations.Count == 0)
                {
                    querySettings.DefaultEnableFilter = filterAttribute.DefaultEnableFilter;
                }
            }
        }
    }
}
