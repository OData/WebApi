//-----------------------------------------------------------------------------
// <copyright file="OrderByAttributeEdmTypeConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class OrderByAttributeEdmTypeConvention : AttributeEdmTypeConvention<StructuralTypeConfiguration>
    {
        public OrderByAttributeEdmTypeConvention()
            : base(attribute => attribute.GetType() == typeof(OrderByAttribute), allowMultiple: true)
        {
        }

        /// <summary>
        /// Set whether the $orderby can be applied on those properties of this structural type.
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
                OrderByAttribute orderByAttribute = attribute as OrderByAttribute;
                ModelBoundQuerySettings querySettings =
                    edmTypeConfiguration.QueryConfiguration.GetModelBoundQuerySettingsOrDefault();
                if (querySettings.OrderByConfigurations.Count == 0)
                {
                    querySettings.CopyOrderByConfigurations(
                        orderByAttribute.OrderByConfigurations);
                }
                else
                {
                    foreach (var property in orderByAttribute.OrderByConfigurations.Keys)
                    {
                        querySettings.OrderByConfigurations[property] =
                            orderByAttribute.OrderByConfigurations[property];
                    }
                }

                if (orderByAttribute.OrderByConfigurations.Count == 0)
                {
                    querySettings.DefaultEnableOrderBy =
                        orderByAttribute.DefaultEnableOrderBy;
                }
            }
        }
    }
}
