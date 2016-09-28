// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.OData.Common;

namespace Microsoft.AspNetCore.OData.Builder.Conventions.Attributes
{
    /// <summary>
    /// Configures classes that have the <see cref="DataContractAttribute"/> to follow DataContract serialization/deserialization rules.
    /// </summary>
    internal class DataContractAttributeEnumTypeConvention : AttributeEdmTypeConvention<EnumTypeConfiguration>
    {
        public DataContractAttributeEnumTypeConvention()
            : base(attribute => attribute.GetType() == typeof(DataContractAttribute), allowMultiple: false)
        {
        }

        /// <summary>
        /// Removes properties that do not have the <see cref="DataMemberAttribute"/> attribute from the enum type.
        /// </summary>
        /// <param name="enumTypeConfiguration">The enum type to configure.</param>
        /// <param name="model">The enum model that this type belongs to.</param>
        /// <param name="attribute">The <see cref="Attribute"/> found on this type.</param>
        public override void Apply(EnumTypeConfiguration enumTypeConfiguration, ODataConventionModelBuilder model,
            Attribute attribute)
        {
            if (enumTypeConfiguration == null)
            {
                throw Error.ArgumentNull("enumTypeConfiguration");
            }

            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (!enumTypeConfiguration.AddedExplicitly &&
                model.ModelAliasingEnabled)
            {
                // set the name, and namespace, if not null
                DataContractAttribute dataContractAttribute = attribute as DataContractAttribute;
                if (dataContractAttribute != null)
                {
                    if (dataContractAttribute.Name != null)
                    {
                        enumTypeConfiguration.Name = dataContractAttribute.Name;
                    }

                    if (dataContractAttribute.Namespace != null)
                    {
                        enumTypeConfiguration.Namespace = dataContractAttribute.Namespace;
                    }
                }
                enumTypeConfiguration.AddedExplicitly = false;
            }

            IEnumerable<EnumMemberConfiguration> allMembers = enumTypeConfiguration.Members.ToArray();
            foreach (EnumMemberConfiguration member in allMembers)
            {
                EnumMemberAttribute enumMemberAttribute =
                    enumTypeConfiguration.ClrType.GetTypeInfo().GetField(member.Name)
                        .GetCustomAttributes(typeof(EnumMemberAttribute), inherit: true)
                        .FirstOrDefault() as EnumMemberAttribute;
                if (!member.AddedExplicitly)
                {
                    if (model.ModelAliasingEnabled && enumMemberAttribute != null)
                    {
                        if (!String.IsNullOrWhiteSpace(enumMemberAttribute.Value))
                        {
                            member.Name = enumMemberAttribute.Value;
                        }
                    }
                    else
                    {
                        enumTypeConfiguration.RemoveMember(member.MemberInfo);
                    }
                }
            }
        }
    }
}
