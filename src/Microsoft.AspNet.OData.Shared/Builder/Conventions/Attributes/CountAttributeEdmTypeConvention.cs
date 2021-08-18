//-----------------------------------------------------------------------------
// <copyright file="CountAttributeEdmTypeConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class CountAttributeEdmTypeConvention : AttributeEdmTypeConvention<StructuralTypeConfiguration>
    {
        public CountAttributeEdmTypeConvention()
            : base(attribute => attribute.GetType() == typeof(CountAttribute), allowMultiple: false)
        {
        }

        /// <summary>
        /// Set whether the $count can be applied on the edm type.
        /// </summary>
        /// <param name="edmTypeConfiguration">The edm type to configure.</param>
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
                CountAttribute countAttribute = attribute as CountAttribute;
                if (countAttribute.Disabled)
                {
                    edmTypeConfiguration.QueryConfiguration.GetModelBoundQuerySettingsOrDefault().Countable = false;
                }
                else
                {
                    edmTypeConfiguration.QueryConfiguration.GetModelBoundQuerySettingsOrDefault().Countable = true;
                }
            }
        }
    }
}
