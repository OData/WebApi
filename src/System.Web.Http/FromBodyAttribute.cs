// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Validation;

namespace System.Web.Http
{
    /// <summary>
    /// This attribute is used on action parameters to indicate
    /// they come only from the content body of the incoming <see cref="HttpRequestMessage"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class FromBodyAttribute : ParameterBindingAttribute
    {
        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            if (parameter == null)
            {
                throw Error.ArgumentNull("parameter");
            }

            HttpControllerDescriptor controllerDescriptor = parameter.ActionDescriptor.ControllerDescriptor;

            if (controllerDescriptor == null)
            {
                throw Error.ArgumentNull("controllerDescriptor");
            }

            IEnumerable<MediaTypeFormatter> formatters = controllerDescriptor.Formatters;
            IBodyModelValidator validator = controllerDescriptor.ControllerServices.GetBodyModelValidator();

            return parameter.BindWithFormatter(formatters, validator);
        }
    }
}
