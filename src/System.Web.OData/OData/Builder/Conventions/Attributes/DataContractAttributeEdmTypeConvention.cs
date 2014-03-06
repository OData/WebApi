// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http;

namespace System.Web.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Configures classes that have the <see cref="DataContractAttribute"/> to follow DataContract serialization/deserialization rules.
    /// </summary>
    internal class DataContractAttributeEdmTypeConvention : AttributeEdmTypeConvention<StructuralTypeConfiguration>
    {
        public DataContractAttributeEdmTypeConvention()
            : base(attribute => attribute.GetType() == typeof(DataContractAttribute), allowMultiple: false)
        {
        }

        /// <summary>
        /// Removes properties that do not have the <see cref="DataMemberAttribute"/> attribute from the edm type.
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

            if (!edmTypeConfiguration.AddedExplicitly &&
                model.ModelAliasingEnabled)
            {
                // set the name, and namespace, if not null
                DataContractAttribute dataContractAttribute = attribute as DataContractAttribute;
                if (dataContractAttribute != null)
                {
                    if (dataContractAttribute.Name != null)
                    {
                        edmTypeConfiguration.Name = dataContractAttribute.Name;
                    }

                    if (dataContractAttribute.Namespace != null)
                    {
                        edmTypeConfiguration.Namespace = dataContractAttribute.Namespace;
                    }
                }
                edmTypeConfiguration.AddedExplicitly = false;
            }

            IEnumerable<PropertyConfiguration> allProperties = edmTypeConfiguration.Properties.ToArray();
            foreach (PropertyConfiguration property in allProperties)
            {
                if (!property.PropertyInfo.GetCustomAttributes(typeof(DataMemberAttribute), inherit: true).Any())
                {
                    if (!property.AddedExplicitly)
                    {
                        edmTypeConfiguration.RemoveProperty(property.PropertyInfo);
                    }
                }
            }
        }
    }
}
