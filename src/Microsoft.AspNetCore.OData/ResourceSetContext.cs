// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Contains context information about the resource set currently being serialized.
    /// </summary>
    public partial class ResourceSetContext
    {
        private HttpRequest _request;

        /// <summary>
        /// Gets or sets the HTTP request that caused this instance to be generated.
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
        /// Gets or sets the <see cref="IEdmModel"/> to which this instance belongs.
        /// </summary>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public IEdmModel EdmModel
        {
            get { return Request.GetModel(); }
        }

        /// <summary>
        /// Create a <see cref="ResourceSetContext"/> from an <see cref="ODataSerializerContext"/> and <see cref="IEnumerable"/>.
        /// </summary>
        /// <param name="resourceSetInstance">The instance representing the resourceSet being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>A new <see cref="ResourceSetContext"/>.</returns>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        internal static ResourceSetContext Create(ODataSerializerContext writeContext, IEnumerable resourceSetInstance)
        {
            ResourceSetContext resourceSetContext = new ResourceSetContext
            {
                Request = writeContext.Request,
                EntitySetBase = writeContext.NavigationSource as IEdmEntitySetBase,
                ResourceSetInstance = resourceSetInstance
            };

            return resourceSetContext;
        }
    }
}
