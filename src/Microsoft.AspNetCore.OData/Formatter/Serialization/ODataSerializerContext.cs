// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;

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
        /// <param name="context">The context from which to copy properties.</param>
        private void CopyProperties(ODataSerializerContext context)
        {
            Request = context.Request;
            Model = context.Model;
            Path = context.Path;
            RootElementName = context.RootElementName;
            SkipExpensiveAvailabilityChecks = context.SkipExpensiveAvailabilityChecks;
            MetadataLevel = context.MetadataLevel;
            Items = context.Items;
        }
    }
}
