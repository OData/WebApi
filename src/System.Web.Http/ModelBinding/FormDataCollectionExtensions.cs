// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Properties;
using System.Web.Http.Validation;
using System.Web.Http.Validation.Providers;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace System.Web.Http.ModelBinding
{
    public static class FormDataCollectionExtensions
    {
        // This is a helper method to use Model Binding over a JQuery syntax. 
        // Normalize from JQuery to MVC keys. The model binding infrastructure uses MVC keys
        // x[] --> x
        // [] --> ""
        // x[field]  --> x.field, where field is not a number
        internal static string NormalizeJQueryToMvc(string key)
        {
            if (key == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            int i = 0;
            while (true)
            {
                int indexOpen = key.IndexOf('[', i);
                if (indexOpen < 0)
                {
                    sb.Append(key, i, key.Length - i);
                    break; // no more brackets
                }

                sb.Append(key, i, indexOpen - i); // everything up to "["

                // Find closing bracket.
                int indexClose = key.IndexOf(']', indexOpen);
                if (indexClose == -1)
                {
                    throw Error.Argument("key", SRResources.JQuerySyntaxMissingClosingBracket);
                }

                if (indexClose == indexOpen + 1)
                {
                    // Empty bracket. Signifies array. Just remove. 
                }
                else
                {
                    if (char.IsDigit(key[indexOpen + 1]))
                    {
                        // array index. Leave unchanged. 
                        sb.Append(key, indexOpen, indexClose - indexOpen + 1);
                    }
                    else
                    {
                        // Field name.  Convert to dot notation. 
                        sb.Append('.');
                        sb.Append(key, indexOpen + 1, indexClose - indexOpen - 1);
                    }
                }

                i = indexClose + 1;
                if (i >= key.Length)
                {
                    break; // end of string
                }
            }
            return sb.ToString();
        }

        internal static NameValueCollection GetJQueryValueNameValueCollection(this FormDataCollection formData)
        {
            if (formData == null)
            {
                throw Error.ArgumentNull("formData");
            }

            NameValueCollection nvc = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in formData)
            {
                ThrowIfMaxHttpCollectionKeysExceeded(nvc.Count);

                string key = NormalizeJQueryToMvc(kv.Key);
                string value = kv.Value ?? String.Empty;                
                nvc.Add(key, value);
            }
            return nvc;
        }

        private static void ThrowIfMaxHttpCollectionKeysExceeded(int count)
        {
            if (count >= MediaTypeFormatter.MaxHttpCollectionKeys)
            {
                throw Error.InvalidOperation(SRResources.MaxHttpCollectionKeyLimitReached, MediaTypeFormatter.MaxHttpCollectionKeys, typeof(MediaTypeFormatter));
            }
        }

        // Create a IValueProvider for the given form, assuming a JQuery syntax.
        internal static IValueProvider GetJQueryValueProvider(this FormDataCollection formData)
        {
            if (formData == null)
            {
                throw Error.ArgumentNull("formData");
            }

            NameValueCollection nvc = formData.GetJQueryValueNameValueCollection();
            return new NameValueCollectionValueProvider(nvc, CultureInfo.InvariantCulture);
        }

        public static T ReadAs<T>(this FormDataCollection formData)
        {
            return (T)ReadAs(formData, typeof(T));
        }
                
        public static object ReadAs(this FormDataCollection formData, Type type)
        {
            return ReadAs(formData, type, string.Empty, requiredMemberSelector: null, formatterLogger: null);
        }

        public static T ReadAs<T>(this FormDataCollection formData, string modelName, IRequiredMemberSelector requiredMemberSelector, IFormatterLogger formatterLogger)
        {
            return (T)ReadAs(formData, typeof(T), modelName, requiredMemberSelector, formatterLogger);
        }
        
        /// <summary>
        /// Deserialize the form data to the given type, using model binding.  
        /// </summary>
        /// <param name="formData">collection with parsed form url data</param>
        /// <param name="type">target type to read as</param>
        /// <param name="modelName">null or empty to read the entire form as a single object. This is common for body data. 
        /// <param name="requiredMemberSelector">The <see cref="IRequiredMemberSelector"/> used to determine required members.</param>
        /// <param name="formatterLogger">The <see cref="IFormatterLogger"/> to log events to.</param>
        /// Or the name of a model to do a partial binding against the form data. This is common for extracting individual fields.</param>
        /// <returns>best attempt to bind the object. The best attempt may be null.</returns>
        public static object ReadAs(this FormDataCollection formData, Type type, string modelName, IRequiredMemberSelector requiredMemberSelector, IFormatterLogger formatterLogger)
        {
            if (formData == null)
            {
                throw Error.ArgumentNull("formData");
            }
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (modelName == null)
            {
                modelName = string.Empty;
            }

            using (HttpConfiguration config = new HttpConfiguration())
            {
                bool validateRequiredMembers = requiredMemberSelector != null && formatterLogger != null;
                if (validateRequiredMembers)
                {
                    // Set a ModelValidatorProvider that understands the IRequiredMemberSelector
                    config.Services.Replace(typeof(ModelValidatorProvider), new RequiredMemberModelValidatorProvider(requiredMemberSelector));
                }

                // Looks like HttpActionContext is just a way of getting to the config, which we really
                // just need to get a list of modelbinderPRoviders for composition. 
                HttpControllerContext controllerContext = new HttpControllerContext() { Configuration = config };
                HttpActionContext actionContext = new HttpActionContext { ControllerContext = controllerContext };

                ModelMetadataProvider metadataProvider = config.Services.GetModelMetadataProvider();

                // Create default over config
                IEnumerable<ModelBinderProvider> providers = config.Services.GetModelBinderProviders();
                ModelBinderProvider modelBinderProvider = new CompositeModelBinderProvider(providers);

                IValueProvider vp = formData.GetJQueryValueProvider();

                ModelBindingContext ctx = new ModelBindingContext()
                {
                    ModelName = modelName,
                    FallbackToEmptyPrefix = false,
                    ModelMetadata = metadataProvider.GetMetadataForType(null, type),
                    ModelState = actionContext.ModelState,
                    ValueProvider = vp
                };

                IModelBinder binder = modelBinderProvider.GetBinder(actionContext, ctx);

                bool haveResult = binder.BindModel(actionContext, ctx);

                // Log model binding errors
                if (validateRequiredMembers)
                {
                    Contract.Assert(formatterLogger != null);
                    foreach (KeyValuePair<string, ModelState> modelStatePair in actionContext.ModelState)
                    {
                        foreach (ModelError modelError in modelStatePair.Value.Errors)
                        {
                            formatterLogger.LogError(modelStatePair.Key, modelError.ErrorMessage);
                        }
                    }
                }

                if (haveResult)
                {
                    return ctx.Model;
                }
                return null;
            }
        }
    }
}
