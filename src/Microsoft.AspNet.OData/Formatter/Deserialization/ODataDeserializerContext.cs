// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Adapters;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// This class encapsulates the state and settings that get passed to <see cref="ODataDeserializer"/>
    /// from the <see cref="ODataMediaTypeFormatter"/>.
    /// </summary>
    public partial class ODataDeserializerContext
    {
        private HttpRequestMessage _request;

        /// <summary>
        /// Gets or sets the HTTP Request that is being deserialized.
        /// </summary>
        public HttpRequestMessage Request
        {
            get { return _request; }
            set
            {
                _request = value;
                InternalRequest = _request != null ? new WebApiRequestMessage(_request) : null;
            }
        }

        /// <summary>Gets or sets the request context.</summary>
        public HttpRequestContext RequestContext { get; set; }
    }
}
