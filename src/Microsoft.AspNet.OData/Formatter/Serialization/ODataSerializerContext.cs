// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Adapters;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// Context information used by the <see cref="ODataSerializer"/> when serializing objects in OData message format.
    /// </summary>
    public partial class ODataSerializerContext
    {
        private HttpRequestMessage _request;
        private UrlHelper _urlHelper;

        /// <summary>
        /// Gets or sets the HTTP Request whose response is being serialized.
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

        /// <summary>
        /// Gets or sets the <see cref="UrlHelper"/> to use for generating OData links.
        /// </summary>
        public UrlHelper Url
        {
            get { return _urlHelper; }

            set
            {
                _urlHelper = value;
                InternalUrlHelper = _urlHelper != null ? new WebApiUrlHelper(_urlHelper) : null;
            }
        }

        /// <summary>
        /// Copy the properties this instance of <see cref="ODataSerializerContext"/> from an existing instance.
        /// </summary>
        /// <param name="context">The context from which to copy properties.</param>
        private void CopyProperties(ODataSerializerContext context)
        {
            Request = context.Request;
            Url = context.Url;
            Model = context.Model;
            Path = context.Path;
            RootElementName = context.RootElementName;
            SkipExpensiveAvailabilityChecks = context.SkipExpensiveAvailabilityChecks;
            MetadataLevel = context.MetadataLevel;
            Items = context.Items;
        }
    }
}
