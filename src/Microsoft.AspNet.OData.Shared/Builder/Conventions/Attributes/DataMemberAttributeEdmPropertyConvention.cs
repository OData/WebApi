﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Configures properties that have <see cref="DataMemberAttribute"/> as optional or required on their edm type.
    /// </summary>
    internal class DataMemberAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public DataMemberAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(DataMemberAttribute), allowMultiple: false)
        {
        }

        public override void Apply(PropertyConfiguration edmProperty,
            StructuralTypeConfiguration structuralTypeConfiguration,
            Attribute attribute,
            ODataConventionModelBuilder model)
        {
            if (structuralTypeConfiguration == null)
            {
                throw Error.ArgumentNull("structuralTypeConfiguration");
            }

            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            bool isTypeDataContract = TypeHelper.AsMemberInfo(structuralTypeConfiguration.ClrType).GetCustomAttributes(typeof(DataContractAttribute), inherit: true).Any();
            DataMemberAttribute dataMember = attribute as DataMemberAttribute;

            if (isTypeDataContract && dataMember != null && !edmProperty.AddedExplicitly)
            {
                // set the name alias
                if (model.ModelAliasingEnabled &&
                    !String.IsNullOrWhiteSpace(dataMember.Name))
                {
                    edmProperty.Name = dataMember.Name;
                }

                StructuralPropertyConfiguration structuralProperty = edmProperty as StructuralPropertyConfiguration;
                if (structuralProperty != null)
                {
                    structuralProperty.OptionalProperty = !dataMember.IsRequired;
                }

                NavigationPropertyConfiguration navigationProperty = edmProperty as NavigationPropertyConfiguration;
                if (navigationProperty != null && navigationProperty.Multiplicity != EdmMultiplicity.Many)
                {
                    if (dataMember.IsRequired)
                    {
                        navigationProperty.Required();
                    }
                    else
                    {
                        navigationProperty.Optional();
                    }
                }
            }
        }
    }
}
