// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Properties;

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// This attribute ensures that The ODataFormatterParameterBinding is used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public sealed class ODataParameterBindingAttribute : ParameterBindingAttribute
    {
        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            IEnumerable<MediaTypeFormatter> formatters = parameter.Configuration.GetODataFormatters();
            Contract.Assert(formatters != null);

            if (formatters.Count() == 0)
            {
                throw Error.Argument("parameter", SRResources.NoODataMediaTypeFormatterFound);
            }

            return new ODataFormatterParameterBinding(parameter, formatters);
        }
    }
}
