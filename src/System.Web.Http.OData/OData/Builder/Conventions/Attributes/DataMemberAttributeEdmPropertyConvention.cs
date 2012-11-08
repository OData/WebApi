// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
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

        public override void Apply(PropertyConfiguration edmProperty, StructuralTypeConfiguration structuralTypeConfiguration, Attribute attribute)
        {
            if (structuralTypeConfiguration == null)
            {
                throw Error.ArgumentNull("structuralTypeConfiguration");
            }

            if (edmProperty == null)
            {
                throw Error.ArgumentNull("edmProperty");
            }

            bool isTypeDataContract = structuralTypeConfiguration.ClrType.GetCustomAttributes(typeof(DataContractAttribute), inherit: true).Any();
            DataMemberAttribute dataMember = attribute as DataMemberAttribute;

            if (isTypeDataContract && dataMember != null && !edmProperty.AddedExplicitly)
            {
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
