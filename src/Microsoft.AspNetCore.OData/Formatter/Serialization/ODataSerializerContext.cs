//-----------------------------------------------------------------------------
// <copyright file="ODataSerializerContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// Context information used by the <see cref="ODataSerializer"/> when serializing objects in OData message format.
    /// </summary>
    public partial class ODataSerializerContext
    {
        private HttpRequest _request;
        private IUrlHelper _urlHelper;

        /// <summary>
        /// Gets or sets the HTTP Request whose response is being serialized.
        /// </summary>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public HttpRequest Request
        {
            get
            {
                return _request;
            }
            set
            {
                _request = value;
                InternalRequest = _request != null ? new WebApiRequestMessage(_request) : null;
                Url = _request != null ? _request.GetUrlHelper() : null;
            }
        }

        /// <summary>
        /// Clone this instance of <see cref="ODataSerializerContext"/> from an existing instance.
        /// </summary>
        /// <param name="context"></param>
        /// <remarks>This function uses types that are AspNetCore-specific.</remarks>
        private void CopyPlatformSpecificProperties(ODataSerializerContext context)
        {
            Request = context.Request;
        }

        /// <summary>
        /// Gets or sets the <see cref="IUrlHelper"/> to use for generating OData links.
        /// </summary>
        public IUrlHelper Url
        {
            get
            {
                return _urlHelper;
            }
            set
            {
                _urlHelper = value;
                InternalUrlHelper = value != null ? new WebApiUrlHelper(value) : null;
            }
        }
    }
}
