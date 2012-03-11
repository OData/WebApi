using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

namespace System.Web.Http.Validation
{
    public abstract class ModelValidatorProvider
    {
        public abstract IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, IEnumerable<ModelValidatorProvider> validatorProviders);
    }
}
