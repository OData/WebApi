//-----------------------------------------------------------------------------
// <copyright file="ExpandAttributeEdmTypeConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class ExpandAttributeEdmTypeConvention : AttributeEdmTypeConvention<StructuralTypeConfiguration>
    {
        public ExpandAttributeEdmTypeConvention()
            : base(attribute => attribute.GetType() == typeof(ExpandAttribute), allowMultiple: true)
        {
        }

        /// <summary>
        /// Set the <see cref="ExpandConfiguration"/>s of navigation properties of this structural type.
        /// </summary>
        /// <param name="edmTypeConfiguration">The entity type to configure.</param>
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
                ExpandAttribute expandAttribute = attribute as ExpandAttribute;
                ModelBoundQuerySettings querySettings =
                    edmTypeConfiguration.QueryConfiguration.GetModelBoundQuerySettingsOrDefault();
                if (querySettings.ExpandConfigurations.Count == 0)
                {
                    querySettings.CopyExpandConfigurations(
                        expandAttribute.ExpandConfigurations);
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
