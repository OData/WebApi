// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Validation;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Describes a parameter that gets bound via ModelBinding.  
    /// </summary>
    public class ModelBinderParameterBinding : HttpParameterBinding
    {
        private readonly ValueProviderFactory[] _valueProviderFactories;        
        private readonly IModelBinder _binder;

        // Cache information for ModelBindingContext.
        private ModelMetadata _metadataCache;
        private ModelValidationNode _validationNodeCache;

        public ModelBinderParameterBinding(HttpParameterDescriptor descriptor,
            IModelBinder modelBinder,
            IEnumerable<ValueProviderFactory> valueProviderFactories)
            : base(descriptor)
        {
            if (modelBinder == null)
            {
                throw Error.ArgumentNull("modelBinder");
            }
            if (valueProviderFactories == null)
            {
                throw Error.ArgumentNull("valueProviderFactories");
            }

            _binder = modelBinder;
            _valueProviderFactories = valueProviderFactories.ToArray();            
        }
        
        public IEnumerable<ValueProviderFactory> ValueProviderFactories
        {
            get { return _valueProviderFactories; }
        }

        public IModelBinder Binder
        {
            get { return _binder; }
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            string name = Descriptor.ParameterName;

            ModelBindingContext ctx = GetModelBindingContext(metadataProvider, actionContext);

            bool haveResult = _binder.BindModel(actionContext, ctx);
            object model = haveResult ? ctx.Model : Descriptor.DefaultValue;
            actionContext.ActionArguments.Add(name, model);

            return TaskHelpers.Completed();
        }

        private ModelBindingContext GetModelBindingContext(ModelMetadataProvider metadataProvider, HttpActionContext actionContext)
        {
            string name = Descriptor.ParameterName;
            Type type = Descriptor.ParameterType;

            string prefix = Descriptor.Prefix;

            IValueProvider vp = CreateValueProvider(this._valueProviderFactories, actionContext);

            if (_metadataCache == null)
            {
                Interlocked.Exchange(ref _metadataCache, metadataProvider.GetMetadataForType(null, type));
            }

            ModelBindingContext ctx = new ModelBindingContext()
            {
                ModelName = prefix ?? name,
                FallbackToEmptyPrefix = prefix == null, // only fall back if prefix not specified
                ModelMetadata = _metadataCache,
                ModelState = actionContext.ModelState,
                ValueProvider = vp
            };
            
            if (_validationNodeCache == null)
            {
                Interlocked.Exchange(ref _validationNodeCache, ctx.ValidationNode);
            }
            else
            {
                ctx.ValidationNode = _validationNodeCache;
            }

            return ctx;
        }

        // Instantiate the value providers for the given action context.
        private static IValueProvider CreateValueProvider(ValueProviderFactory[] factories, HttpActionContext actionContext)
        {
            if (factories.Length == 1)
            {
                return factories[0].GetValueProvider(actionContext);
            }

            IValueProvider[] providers = Array.ConvertAll(factories, f => f.GetValueProvider(actionContext));            
            return new CompositeValueProvider(providers);
        }
    }
}
