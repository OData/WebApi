using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http.Common;
using System.Web.Http.Metadata;

namespace System.Web.Http.Validation
{
    /// <summary>
    /// This <see cref="IRequiredMemberSelector"/> selects required members by checking for any 
    /// required ModelValidators associated with the member. This is the default implementation used by
    /// <see cref="HttpConfiguration"/>.
    /// </summary>
    public class ModelValidationRequiredMemberSelector : IRequiredMemberSelector
    {
        private readonly HttpConfiguration _configuration;

        public ModelValidationRequiredMemberSelector(HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("dependencyResolver");
            }

            _configuration = configuration;
        }

        public bool IsRequiredMember(MemberInfo member)
        {
            if (member == null)
            {
                throw Error.ArgumentNull("member");
            }

            if (!(member is PropertyInfo))
            {
                return false;
            }
            ModelMetadataProvider metadataProvider = _configuration.ServiceResolver.GetModelMetadataProvider();
            IEnumerable<ModelValidatorProvider> validatorProviders = _configuration.ServiceResolver.GetModelValidatorProviders();

            ModelMetadata metadata = metadataProvider.GetMetadataForProperty(() => null, member.DeclaringType, member.Name);
            IEnumerable<ModelValidator> validators = metadata.GetValidators(validatorProviders);
            return validators.Any(validator => validator.IsRequired);
        }
    }
}
