// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Exposes the ability to parse an OData path as an <see cref="ODataPath"/> and convert an <see cref="ODataPath"/> into an OData link.
    /// </summary>
    public interface IODataPathHandler
    {
        /// <summary>
        /// Parses the specified OData path as an <see cref="ODataPath"/> that contains additional information about the EDM type and entity set for the path.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="serviceRoot">The service root of the OData path.</param>
        /// <param name="path">The OData path to parse.</param>
        /// <returns>A parsed representation of the URI, or <c>null</c> if the URI does not match the model.</returns>
        ODataPath Parse(IEdmModel model, string serviceRoot, string path);

        /// <summary>
        /// Converts an instance of <see cref="ODataPath"/> into an OData link.
        /// </summary>
        /// <param name="path">The OData path to convert into a link.</param>
        /// <returns>The generated OData link.</returns>
        string Link(ODataPath path);
    }
}
