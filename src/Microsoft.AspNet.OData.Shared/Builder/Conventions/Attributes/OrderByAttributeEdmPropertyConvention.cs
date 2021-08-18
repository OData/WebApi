//-----------------------------------------------------------------------------
// <copyright file="OrderByAttributeEdmPropertyConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class OrderByAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public OrderByAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(OrderByAttribute), allowMultiple: true)
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
                OrderByAttribute orderByAttribute = attribute as OrderByAttribute;
                ModelBoundQuerySettings querySettings =
                    edmProperty.QueryConfiguration.GetModelBoundQuerySettingsOrDefault();
                if (querySettings.OrderByConfigurations.Count == 0)
                {
                    querySettings.CopyOrderByConfigurations(orderByAttribute.OrderByConfigurations);
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
                    querySettings.DefaultEnableOrderBy = orderByAttribute.DefaultEnableOrderBy;
                }
            }
        }
    }
}
