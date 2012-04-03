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
            get { return _providers;  }
        }

        public override IModelBinder GetBinder(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            // Extract all providers from the resolver except the the type of the executing one (else would cause recursion),
            // or use the set of providers we were given.
            IEnumerable<ModelBinderProvider> providers = _providers != null
                                                             ? _providers
                                                             : actionContext.ControllerContext.Configuration.Services.GetModelBinderProviders().Where((p) => !typeof(CompositeModelBinderProvider).IsAssignableFrom(p.GetType()));

            return new CompositeModelBinder(providers);
        }
    }
}
