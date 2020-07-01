// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// An attribute to disable WebApi model validation for a particular type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
    internal sealed partial class NonValidatingParameterBindingAttribute : ParameterBindingAttribute
    {
        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            IEnumerable<MediaTypeFormatter> formatters = parameter.Configuration.Formatters;

            return new NonValidatingParameterBinding(parameter, formatters);
        }

        private sealed class NonValidatingParameterBinding : PerRequestParameterBinding
        {
            public NonValidatingParameterBinding(HttpParameterDescriptor descriptor,
                IEnumerable<MediaTypeFormatter> formatters)
                : base(descriptor, formatters)
            {
            }

            protected override HttpParameterBinding CreateInnerBinding(IEnumerable<MediaTypeFormatter> perRequestFormatters)
            {
                return Descriptor.BindWithFormatter(perRequestFormatters, bodyModelValidator: null);
            }
        }
    }
}
