// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Parses an OData path as an <see cref="ODataPath"/> and converts an <see cref="ODataPath"/> into an OData link.
    /// </summary>
    public class DefaultODataPathHandler : IODataPathHandler
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultODataPathHandler"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service collection.</param>
        public DefaultODataPathHandler(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull("serviceProvider");
            }

            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Parses the specified OData path as an <see cref="ODataPath"/> that contains additional information about the EDM type and entity set for the path.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="serviceRoot">The service root of the OData path.</param>
        /// <param name="path">The OData path to parse.</param>
        /// <returns>A parsed representation of the path, or <c>null</c> if the path does not match the model.</returns>
        public virtual ODataPath Parse(IEdmModel model, string serviceRoot, string path)
        {
            if (serviceRoot == null)
            {
                throw Error.ArgumentNull("serviceRoot");
            }

            if (path == null)
            {
                throw Error.ArgumentNull("path");
            }

            return ODataPathParserHelper.Parse(model, serviceRoot, path, false, _serviceProvider);
        }

        /// <summary>
        /// Converts an instance of <see cref="ODataPath" /> into an OData link.
        /// </summary>
        /// <param name="path">The OData path to convert into a link.</param>
        /// <returns>
        /// The generated OData link.
        /// </returns>
        public virtual string Link(ODataPath path)
        {
            if (path == null)
            {
                throw Error.ArgumentNull("path");
            }

            return path.ToString();
        }
    }
}
