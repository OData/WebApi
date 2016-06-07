// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace System.Web.OData
{
    internal class ODataUriResolverSettings
    {
        /// <summary>
        /// Gets or sets the resolver for Uri parsing
        /// </summary>
        public ODataUriResolver UriResolver { get; set; }

        /// <summary>
        /// Gets or Sets the <see cref="ODataUrlConventions"/> to use while parsing, specifically
        /// whether to recognize keys as segments or not.
        /// </summary>
        public ODataUrlConventions UrlConventions { get; set; }
    }
}
