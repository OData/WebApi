// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
            ODataMediaTypeFormatter formatter = parameter.Configuration.GetODataFormatter();
            if (formatter == null)
            {
                throw Error.InvalidOperation(SRResources.NoODataMediaTypeFormatterFound);
            }
            return new ODataFormatterParameterBinding(parameter, formatter);
        }
    }
}
