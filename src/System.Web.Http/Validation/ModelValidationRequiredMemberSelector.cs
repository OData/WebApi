// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http.Metadata;

namespace System.Web.Http.Validation
{
    /// <summary>
    /// This <see cref="IRequiredMemberSelector"/> selects required members by checking for any 
    /// required ModelValidators associated with the member. This is the default implementation used by
    /// <see cref="HttpConfiguration"/>.
    /// </summary>
    public sealed class ModelValidationRequiredMemberSelector : IRequiredMemberSelector
    {
        private readonly ModelMetadataProvider _metadataProvider;
        private readonly List<ModelValidatorProvider> _validatorProviders;

        public ModelValidationRequiredMemberSelector(ModelMetadataProvider metadataProvider, IEnumerable<ModelValidatorProvider> validatorProviders)
        {
            if (metadataProvider == null)
            {
                throw Error.ArgumentNull("metadataProvider");
            }
            if (validatorProviders == null)
            {
                throw Error.ArgumentNull("validatorProviders");
            }

            _metadataProvider = metadataProvider;
            _validatorProviders = validatorProviders.ToList();
        }

        public bool IsRequiredMember(MemberInfo member)
        {
            if (member == null)
            {
                throw Error.ArgumentNull("member");
            }

            // Optimization : avoid computing validators if there are no validator providers
            if (_validatorProviders == null || !_validatorProviders.Any())
            {
                return false;
            }

            if (!(member is PropertyInfo))
            {
                return false;
            }

            ModelMetadata metadata = _metadataProvider.GetMetadataForProperty(() => null, member.DeclaringType, member.Name);
            IEnumerable<ModelValidator> validators = metadata.GetValidators(_validatorProviders);
            return validators.Any(validator => validator.IsRequired);
        }
    }
}
