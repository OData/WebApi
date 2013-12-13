// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using System.Web.Http.ValueProviders;

namespace System.Web.Http.OData
{
    /// <summary>
    /// An implementation of <see cref="ParameterBindingAttribute"/> that can bind URI parameters using OData conventions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class FromODataUriAttribute : ModelBinderAttribute
    {
        private static readonly ModelBinderProvider _provider = new ODataModelBinderProvider();

        /// <summary>
        /// Gets the binding for a parameter.
        /// </summary>
        /// <param name="parameter">The parameter to bind.</param>
        /// <returns>
        /// The <see cref="T:System.Web.Http.Controllers.HttpParameterBinding" />that contains the binding.
        /// </returns>
        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            if (parameter == null)
            {
                throw Error.ArgumentNull("parameter");
            }

            IModelBinder binder = _provider.GetBinder(parameter.Configuration, parameter.ParameterType);
            if (binder == null)
            {
                throw Error.Argument("parameter", SRResources.FromODataUriRequiresPrimitive, parameter.ParameterType.FullName);
            }

            IEnumerable<ValueProviderFactory> valueProviderFactories = GetValueProviderFactories(parameter.Configuration);
            return new ModelBinderParameterBinding(parameter, binder, valueProviderFactories); 
        }
    }
}
