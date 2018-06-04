// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    internal class SelectAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public SelectAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(SelectAttribute), allowMultiple: true)
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
                SelectAttribute selectAttribute = attribute as SelectAttribute;
                ModelBoundQuerySettings querySettings =
                    edmProperty.QueryConfiguration.GetModelBoundQuerySettingsOrDefault();
                if (querySettings.SelectConfigurations.Count == 0)
                {
                    querySettings.CopySelectConfigurations(selectAttribute.SelectConfigurations);
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
                    querySettings.DefaultSelectType = selectAttribute.DefaultSelectType;
                }
            }
        }
    }
}
