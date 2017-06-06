// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Query;

namespace System.Web.OData.Builder.Conventions.Attributes
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
