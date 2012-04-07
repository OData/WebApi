// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Properties;
using System.Web.Http.ValueProviders;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Specify this parameter uses a model binder. This can optionally specify the specific model binder and 
    /// value providers that drive that model binder. 
    /// Derived attributes may provide convenience settings for the model binder or value provider. 
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "want constructor argument shortcut")]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "part of a class hierarchy")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public class ModelBinderAttribute : Attribute
    {
        public ModelBinderAttribute()
            : this(null)
        {
        }

        public ModelBinderAttribute(Type binderType)
        {
            BinderType = binderType;
        }

        /// <summary>
        /// Sets the type of the model binder. 
        /// This type must be a subclass of <see cref="ModelBinderProvider"/>        
        /// If null, uses the default from the configuration.
        /// </summary>   
        public Type BinderType { get; set; }

        /// <summary>
        /// Gets or sets the name to consider as the parameter name during model binding
        /// </summary>
        public string Name { get; set; }

        public bool SuppressPrefixCheck { get; set; }

        // This will get called by a parameter binding, which will cache the results. 
        public ModelBinderProvider GetModelBinderProvider(HttpConfiguration configuration)
        {
            if (BinderType != null)
            {
                object value = configuration.DependencyResolver.GetService(BinderType)
                            ?? Activator.CreateInstance(BinderType);

                if (value != null)
                {
                    VerifyBinderType(value.GetType());
                    ModelBinderProvider result = (ModelBinderProvider)value;
                    return result;
                }
            }

            // Create default over config
            IEnumerable<ModelBinderProvider> providers = configuration.Services.GetModelBinderProviders();

            if (providers.Count() == 1)
            {
                return providers.First();
            }

            return new CompositeModelBinderProvider(providers);
        }

        /// <summary>
        /// Value providers that will be fed to the model binder.
        /// </summary>
        public virtual IEnumerable<ValueProviderFactory> GetValueProviderFactories(HttpConfiguration configuration)
        {
            // By default, just get all registered value provider factories
            return configuration.Services.GetValueProviderFactories();
        }

        private static void VerifyBinderType(Type attemptedType)
        {
            Type required = typeof(ModelBinderProvider);
            if (!required.IsAssignableFrom(attemptedType))
            {
                throw Error.InvalidOperation(SRResources.ValueProviderFactory_Cannot_Create, required.Name, attemptedType.Name, required.Name);
            }
        }
    }
}
