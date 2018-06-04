﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;

namespace Microsoft.AspNet.OData
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
            IEnumerable<ValueProviderFactory> valueProviderFactories = GetValueProviderFactories(parameter.Configuration);
            return new ModelBinderParameterBinding(parameter, binder, valueProviderFactories);
        }
    }
}
