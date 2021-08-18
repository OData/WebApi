//-----------------------------------------------------------------------------
// <copyright file="ResourceSetContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Contains context information about the resource set currently being serialized.
    /// </summary>
    public partial class ResourceSetContext
    {
        private HttpRequest _request;
        private IUrlHelper _urlHelper;

        /// <summary>
        /// Gets or sets the HTTP request that caused this instance to be generated.
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
                Url = _request != null ? Request.GetUrlHelper() : null;
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
                Url = writeContext.Url,
                ResourceSetInstance = resourceSetInstance
            };

            return resourceSetContext;
        }
    }
}
