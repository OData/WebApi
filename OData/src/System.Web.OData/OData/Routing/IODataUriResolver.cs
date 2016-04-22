// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Metadata;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// Exposes the ability to set the Uri resolver settings.
    /// </summary>
    public interface IODataUriResolver
    {
        /// <summary>
        /// Gets or sets the resolver for Uri parsing
        /// </summary>
        ODataUriResolver UriResolver { get; set; }

        /// <summary>
        /// Gets or Sets the <see cref="ODataUrlConventions"/> to use while parsing, specifically
        /// whether to recognize keys as segments or not.
        /// </summary>
        ODataUrlConventions UrlConventions { get; set; }
    }
}