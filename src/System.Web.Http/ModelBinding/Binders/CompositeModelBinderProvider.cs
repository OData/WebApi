// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding.Binders
{
    public sealed class CompositeModelBinderProvider : ModelBinderProvider
    {
        private ModelBinderProvider[] _providers;

        public CompositeModelBinderProvider()
        {
        }

        public CompositeModelBinderProvider(IEnumerable<ModelBinderProvider> providers)
        {
            if (providers == null)
            {
                throw Error.ArgumentNull("providers");
            }

            _providers = providers.ToArray();
        }

        public IEnumerable<ModelBinderProvider> Providers
        {
            get { return _providers; }
        }

        public override IModelBinder GetBinder(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            // Fast-path the case where we already have the providers. 
            if (_providers != null)
            {
                return new CompositeModelBinder(_providers);
            }

            // Extract all providers from the resolver except the the type of the executing one (else would cause recursion),
            // or use the set of providers we were given.            
            IEnumerable<ModelBinderProvider> providers = actionContext.ControllerContext.Configuration.Services.GetModelBinderProviders().Where(p => !(p is CompositeModelBinderProvider));

            return new CompositeModelBinder(providers);
        }
    }
}
