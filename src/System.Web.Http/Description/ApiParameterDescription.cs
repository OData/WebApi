// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;

namespace System.Web.Http.Description
{
    /// <summary>
    /// Describes a parameter on the API defined by relative URI path and HTTP method.
    /// </summary>
    public class ApiParameterDescription
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the documentation.
        /// </summary>
        /// <value>
        /// The documentation.
        /// </value>
        public string Documentation { get; set; }

        /// <summary>
        /// Gets or sets the source of the parameter. It may come from the request URI, request body or other places.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public ApiParameterSource Source { get; set; }

        /// <summary>
        /// Gets or sets the parameter descriptor.
        /// </summary>
        /// <value>
        /// The parameter descriptor.
        /// </value>
        public HttpParameterDescriptor ParameterDescriptor { get; set; }
    }
}
