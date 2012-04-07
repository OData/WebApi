// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;
using System.Web.Http.Properties;
using System.Web.Http.Validation;

namespace System.Web.Http.ModelBinding
{
    public class DefaultActionValueBinder : IActionValueBinder
    {
        // Derived binder may customize the formatters and use per-controller specific formatters instead of config formatters.
        protected virtual IEnumerable<MediaTypeFormatter> GetFormatters(HttpActionDescriptor actionDescriptor)
        {
            IEnumerable<MediaTypeFormatter> formatters = actionDescriptor.Configuration.Formatters;
            return formatters;
        }

        protected virtual IBodyModelValidator GetBodyModelValidator(HttpActionDescriptor actionDescriptor)
        {
            return actionDescriptor.Configuration.Services.GetBodyModelValidator();
        }

        /// <summary>
        /// Implementation of <see cref="IActionValueBinder"/>, Primary entry point for binding parameters for an action.
        /// </summary>           
        public virtual HttpActionBinding GetBinding(HttpActionDescriptor actionDescriptor)
        {
            if (actionDescriptor == null)
            {
                throw Error.ArgumentNull("actionDescriptor");
            }            

            HttpParameterDescriptor[] parameters = actionDescriptor.GetParameters().ToArray();
            HttpParameterBinding[] binders = Array.ConvertAll(parameters, DetermineBinding);

            HttpActionBinding actionBinding = new HttpActionBinding(actionDescriptor, binders);
                        
            EnsureOneBodyParameter(actionBinding);

            return actionBinding;            
        }

        /// <summary>
        /// Update actionBinding to enforce there is at most 1 body parameter. 
        /// If there are multiple, convert them all to <see cref="ErrorParameterBinding"/>
        /// </summary>
        private static void EnsureOneBodyParameter(HttpActionBinding actionBinding)
        {
            IList<HttpParameterDescriptor> parameters = actionBinding.ActionDescriptor.GetParameters();

            int idxFromBody = -1;
            for (int i = 0; i < actionBinding.ParameterBindings.Length; i++)
            {
                if (actionBinding.ParameterBindings[i].WillReadBody)
                {
                    if (idxFromBody >= 0)
                    {
                        // This is the 2nd parameter to read from the body. Flag an error.
                        string name1 = parameters[idxFromBody].ParameterName;
                        string name2 = parameters[i].ParameterName;

                        string message = Error.Format(SRResources.ParameterBindingCantHaveMultipleBodyParameters, name1, name2);
                        actionBinding.ParameterBindings[i] = new ErrorParameterBinding(parameters[i], message);
                        actionBinding.ParameterBindings[idxFromBody] = new ErrorParameterBinding(parameters[idxFromBody], message);
                    }
                    else
                    {
                        idxFromBody = i;
                    }
                }
            }
        }

        private HttpParameterBinding MakeBodyBinding(HttpParameterDescriptor parameter)
        {
            HttpActionDescriptor actionDescriptor = parameter.ActionDescriptor;
            return new FormatterParameterBinding(parameter, GetFormatters(actionDescriptor), GetBodyModelValidator(actionDescriptor));
        }

        // Determine how a single parameter will get bound. 
        // This is all sync. We don't need to actually read the body just to determine that we'll bind to the body.         
        private HttpParameterBinding DetermineBinding(HttpParameterDescriptor parameter)
        {
            // Look at Parameter attributes?
            // [FromBody] - we use Formatter.
            bool hasFromBody = parameter.GetCustomAttributes<FromBodyAttribute>().Any();
            ModelBinderAttribute attr = parameter.ModelBinderAttribute;
                        
            if (hasFromBody)
            {
                if (attr != null)
                {
                    string message = Error.Format(SRResources.ParameterBindingConflictingAttributes, parameter.ParameterName);
                    return new ErrorParameterBinding(parameter, message);
                }

                return MakeBodyBinding(parameter); // It's from the body. Uses a formatter. 
            }

            // Presence of a model binder attribute overrides.
            if (attr != null)
            {
                return GetBinding(parameter, attr);
            }
            
            // No attribute, key off type
            Type type = parameter.ParameterType;
            if (TypeHelper.IsSimpleUnderlyingType(type) || TypeHelper.HasStringConverter(type))
            {
                attr = new ModelBinderAttribute(); // use default settings
                return GetBinding(parameter, attr);
            }

            // Handle special types
            if (type == typeof(CancellationToken))
            {
                return new CancellationTokenParameterBinding(parameter);
            }
            if (type == typeof(HttpRequestMessage))
            {
                return new HttpRequestParameterBinding(parameter);
            }
            if (typeof(HttpContent).IsAssignableFrom(type))
            {
                string message = Error.Format(SRResources.ParameterBindingIllegalType, type.Name, parameter.ParameterName);
                return new ErrorParameterBinding(parameter, message);
            }            

            // Fallback. Must be a complex type. Default is to look in body.             
            return MakeBodyBinding(parameter);
        }

        private static HttpParameterBinding GetBinding(HttpParameterDescriptor parameter, ModelBinderAttribute attr)
        {
            HttpConfiguration config = parameter.Configuration;
            return new ModelBinderParameterBinding(parameter,
                attr.GetModelBinderProvider(config),
                attr.GetValueProviderFactories(config));
        }
    }
}
