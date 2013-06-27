// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Description
{
    /// <summary>
    /// Describes the API response.
    /// </summary>
    public class ResponseDescription
    {
        /// <summary>
        /// Gets or sets the declared response type.
        /// </summary>
        public Type DeclaredType { get; set; }

        /// <summary>
        /// Gets or sets the actual response type.
        /// </summary>
        public Type ResponseType { get; set; }

        /// <summary>
        /// Gets or sets the response documentation.
        /// </summary>
        public string Documentation { get; set; }
    }
}