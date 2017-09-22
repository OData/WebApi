// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Interfaces;
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
        private IUrlHelper _urlHelper;

        /// <summary>
        /// Gets or sets the HTTP request that caused this instance to be generated.
        /// </summary>
        public HttpRequest Request
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IWebApiUrlHelper"/> to be used for generating links while serializing this
        /// feed instance.
        /// </summary>
        public IUrlHelper Url
        {
            get { return _urlHelper; }
            set
            {
                _urlHelper = value;
                InternalUrlHelper = _urlHelper != null ? new WebApiUrlHelper(_urlHelper) : null;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IEdmModel"/> to which this instance belongs.
        /// </summary>
        public IEdmModel EdmModel
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the HTTP request that caused this instance to be generated.
        /// </summary>
        internal IWebApiRequestMessage InternalRequest { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IWebApiUrlHelper"/> to be used for generating links while serializing this
        /// feed instance.
        /// </summary>
        internal IWebApiUrlHelper InternalUrlHelper { get; private set; }
    }
}
