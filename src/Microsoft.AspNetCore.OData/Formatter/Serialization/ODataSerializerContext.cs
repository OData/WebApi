// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// Context information used by the <see cref="ODataSerializer"/> when serializing objects in OData message format.
    /// </summary>
    public partial class ODataSerializerContext
    {
        private HttpRequest _request;

        /// <summary>
        /// Gets or sets the HTTP Request whose response is being serialized.
        /// </summary>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public HttpRequest Request
        {
            get { return _request; }

            set
            {
                _request = value;
                InternalRequest = _request != null ? new WebApiRequestMessage(_request) : null;
                InternalUrlHelper = _request != null ? new WebApiUrlHelper(_request.HttpContext.GetUrlHelper()) : null;
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
    }
}
