// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http.Metadata;
using System.Web.Http.Validation.Validators;

namespace System.Web.Http.Validation.Providers
{
    /// <summary>
    /// This <see cref="ModelValidatorProvider"/> provides a required ModelValidator for members marked as [DataMember(IsRequired=true)].
    /// </summary>
    public class DataMemberModelValidatorProvider : AssociatedValidatorProvider
    {
        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders, IEnumerable<Attribute> attributes)
        {
            DataMemberAttribute dataMemberAttribute = attributes.OfType<DataMemberAttribute>().FirstOrDefault();
            if (dataMemberAttribute != null)
            {
                // isDataContract == true iff the containter type has at least one DataContractAttribute
                bool isDataContract = GetTypeDescriptor(metadata.ContainerType).GetAttributes().OfType<DataContractAttribute>().Any();
                if (isDataContract && dataMemberAttribute.IsRequired)
                {
                    return new[] { new RequiredMemberModelValidator(validatorProviders) };
                }
            }
            return new ModelValidator[0];
        }
    }
}
