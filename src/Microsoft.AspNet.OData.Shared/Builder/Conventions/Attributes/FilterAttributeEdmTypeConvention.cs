//-----------------------------------------------------------------------------
// <copyright file="FilterAttributeEdmTypeConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class FilterAttributeEdmTypeConvention : AttributeEdmTypeConvention<StructuralTypeConfiguration>
    {
        public FilterAttributeEdmTypeConvention()
            : base(attribute => attribute.GetType() == typeof(FilterAttribute), allowMultiple: true)
        {
        }

        /// <summary>
        /// Set whether the $filter can be applied on those properties of this structural type.
        /// </summary>
        /// <param name="edmTypeConfiguration">The structural type to configure.</param>
        /// <param name="model">The edm model that this type belongs to.</param>
        /// <param name="attribute">The <see cref="Attribute"/> found on this type.</param>
        public override void Apply(StructuralTypeConfiguration edmTypeConfiguration, ODataConventionModelBuilder model,
            Attribute attribute)
        {
            if (edmTypeConfiguration == null)
            {
                throw Error.ArgumentNull("edmTypeConfiguration");
            }

            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (!edmTypeConfiguration.AddedExplicitly)
            {
                FilterAttribute filterAttribute = attribute as FilterAttribute;
                ModelBoundQuerySettings querySettings =
                    edmTypeConfiguration.QueryConfiguration.GetModelBoundQuerySettingsOrDefault();
                if (querySettings.FilterConfigurations.Count == 0)
                {
                    querySettings.CopyFilterConfigurations(
                        filterAttribute.FilterConfigurations);
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
