using System.Collections.Generic;

namespace System.Web.Mvc
{
    public abstract class ModelValidatorProvider
    {
        public abstract IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context);
    }
}
