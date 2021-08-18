//-----------------------------------------------------------------------------
// <copyright file="IODataPathHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// Exposes the ability to parse an OData path as an <see cref="ODataPath"/> and convert an <see cref="ODataPath"/> into an OData link.
    /// </summary>
    public interface IODataPathHandler
    {
        /// <summary>
        /// Parses the specified OData path as an <see cref="ODataPath"/> that contains additional information about the EDM type and entity set for the path.
        /// </summary>
        /// <param name="serviceRoot">The service root of the OData path.</param>
        /// <param name="odataPath">The OData path to parse.</param>
        /// <param name="requestContainer">The dependency injection container for the request.</param>
        /// <returns>A parsed representation of the URI, or <c>null</c> if the URI does not match the model.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "odata", Justification = "odata is spelled correctly")]
        ODataPath Parse(string serviceRoot, string odataPath, IServiceProvider requestContainer);

        /// <summary>
        /// Converts an instance of <see cref="ODataPath"/> into an OData link.
        /// </summary>
        /// <param name="path">The OData path to convert into a link.</param>
        /// <returns>The generated OData link.</returns>
        string Link(ODataPath path);

        /// <summary>
        /// Gets or Sets the <see cref="ODataUrlKeyDelimiter"/> to use while parsing, specifically
        /// whether to recognize keys as segments or not.
        /// </summary>
        ODataUrlKeyDelimiter UrlKeyDelimiter { get; set; }
    }
}
