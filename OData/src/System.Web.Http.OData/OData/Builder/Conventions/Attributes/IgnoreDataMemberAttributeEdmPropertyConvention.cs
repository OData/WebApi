// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Runtime.Serialization;

namespace System.Web.Http.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Removes properties that have <see cref="IgnoreDataMemberAttribute"/> from their edm type.
    /// </summary>
    internal class IgnoreDataMemberAttributeEdmPropertyConvention : AttributeEdmPropertyConvention<PropertyConfiguration>
    {
        public IgnoreDataMemberAttributeEdmPropertyConvention()
            : base(attribute => attribute.GetType() == typeof(IgnoreDataMemberAttribute), allowMultiple: false)
        {
        }

        /// <summary>
        /// Removes the property from the edm type.
        /// </summary>
        /// <param name="edmProperty">The property being removed.</param>
        /// <param name="structuralTypeConfiguration">The edm type from which the property is being removed.</param>
        /// <param name="attribute">The <see cref="Attribute"/> found on this type.</param>
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

            if (!edmProperty.AddedExplicitly)
            {
                bool isTypeDataContract = structuralTypeConfiguration.ClrType.GetCustomAttributes(typeof(DataContractAttribute), inherit: true).Any();
                bool isPropertyDataMember = edmProperty.PropertyInfo.GetCustomAttributes(typeof(DataMemberAttribute), inherit: true).Any();

                if (isTypeDataContract && isPropertyDataMember)
                {
                    // both Datamember and IgnoreDataMember. DataMember wins as this a DataContract
                    return;
                }
                else
                {
                    structuralTypeConfiguration.RemoveProperty(edmProperty.PropertyInfo);
                }
            }
        }
    }
}
