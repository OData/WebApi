//-----------------------------------------------------------------------------
// <copyright file="SelectAttributeEdmTypeConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class SelectAttributeEdmTypeConvention : AttributeEdmTypeConvention<StructuralTypeConfiguration>
    {
        public SelectAttributeEdmTypeConvention()
            : base(attribute => attribute.GetType() == typeof(SelectAttribute), allowMultiple: true)
        {
        }

        /// <summary>
        /// Set whether the $select can be applied on those properties of this structural type.
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
                SelectAttribute selectAttribute = attribute as SelectAttribute;
                ModelBoundQuerySettings querySettings =
                    edmTypeConfiguration.QueryConfiguration.GetModelBoundQuerySettingsOrDefault();
                if (querySettings.SelectConfigurations.Count == 0)
                {
                    querySettings.CopySelectConfigurations(
                        selectAttribute.SelectConfigurations);
                }
                else
                {
                    foreach (var property in selectAttribute.SelectConfigurations.Keys)
                    {
                        querySettings.SelectConfigurations[property] =
                            selectAttribute.SelectConfigurations[property];
                    }
                }

                if (selectAttribute.SelectConfigurations.Count == 0)
                {
                    querySettings.DefaultSelectType =
                        selectAttribute.DefaultSelectType;
                }
            }
        }
    }
}
