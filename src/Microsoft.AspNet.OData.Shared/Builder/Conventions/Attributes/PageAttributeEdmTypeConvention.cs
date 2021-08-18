//-----------------------------------------------------------------------------
// <copyright file="PageAttributeEdmTypeConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class PageAttributeEdmTypeConvention : AttributeEdmTypeConvention<StructuralTypeConfiguration>
    {
        public PageAttributeEdmTypeConvention()
            : base(attribute => attribute.GetType() == typeof(PageAttribute), allowMultiple: false)
        {
        }

        /// <summary>
        /// Set page size of the entity type.
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
                PageAttribute pageAttribute = attribute as PageAttribute;
                ModelBoundQuerySettings querySettings =
                    edmTypeConfiguration.QueryConfiguration.GetModelBoundQuerySettingsOrDefault();

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
