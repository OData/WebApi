using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Common;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Describes a parameter that gets bound via ModelBinding.  
    /// </summary>
    public class ModelBinderParameterBinding : HttpParameterBinding
    {
        private readonly IEnumerable<ValueProviderFactory> _valueProviderFactories;
        private readonly ModelBinderProvider _modelBinderProvider;

        public ModelBinderParameterBinding(HttpParameterDescriptor descriptor,
            ModelBinderProvider modelBinderProvider,
            IEnumerable<ValueProviderFactory> valueProviderFactories)
            : base(descriptor)
        {
            if (modelBinderProvider == null)
            {
                throw Error.ArgumentNull("modelBinderProvider");
            }
            if (valueProviderFactories == null)
            {
                throw Error.ArgumentNull("valueProviderFactories");
            }

            _modelBinderProvider = modelBinderProvider;
            _valueProviderFactories = valueProviderFactories;
        }

        public IEnumerable<ValueProviderFactory> ValueProviderFactories
        {
            get { return _valueProviderFactories; }
        }

        public ModelBinderProvider ModelBinderProvider
        {
            get { return _modelBinderProvider; }
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            string name = Descriptor.ParameterName;
            Type type = Descriptor.ParameterType;

            string prefix = Descriptor.Prefix;

            IValueProvider vp = CreateValueProvider(this._valueProviderFactories, actionContext);

            ModelBindingContext ctx = new ModelBindingContext()
            {
                ModelName = prefix ?? name,
                FallbackToEmptyPrefix = prefix == null, // only fall back if prefix not specified
                ModelMetadata = metadataProvider.GetMetadataForType(null, type),
                ModelState = actionContext.ModelState,
                ValueProvider = vp
            };

            IModelBinder binder = this._modelBinderProvider.GetBinder(actionContext, ctx);

            bool haveResult = binder.BindModel(actionContext, ctx);
            object model = haveResult ? ctx.Model : Descriptor.DefaultValue;
            actionContext.ActionArguments.Add(name, model);

            return TaskHelpers.Completed();
        }

        // Instantiate the value providers for the given action context.
        private static IValueProvider CreateValueProvider(IEnumerable<ValueProviderFactory> factories, HttpActionContext actionContext)
        {
            List<IValueProvider> providers = factories.Select<ValueProviderFactory, IValueProvider>(f => f.GetValueProvider(actionContext)).ToList();
            if (providers.Count == 1)
            {
                return providers[0];
            }
            return new CompositeValueProvider(providers);
        }
    }
}
