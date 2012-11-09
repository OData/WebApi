// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// Exposes the ability to parse an OData URI as an <see cref="ODataPath"/> that contains additional information about the EDM type and entity set for the path.
    /// </summary>
    public interface IODataPathParser
    {
        /// <summary>
        /// Parses the specified OData URI as an <see cref="ODataPath"/> that contains additional information about the EDM type and entity set for the path.
        /// </summary>
        /// <param name="uri">The OData URI to parse.</param>
        /// <param name="baseUri">The base URI of the service.</param>
        /// <returns>A parsed representation of the URI, or <c>null</c> if the URI does not match the model.</returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Implementations shouldn't need to subclass ODataPath")]
        ODataPath Parse(Uri uri, Uri baseUri);
    }
}