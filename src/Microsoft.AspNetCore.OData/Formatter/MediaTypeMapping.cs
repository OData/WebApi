//-----------------------------------------------------------------------------
// <copyright file="MediaTypeMapping.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// A class to support matching media types.
    /// </summary>
    /// <remarks>
    /// This is part of the platform in AspNet but defined here for AspNetCore to allow for reusing
    /// the classes derive form it for managing media type mapping.
    /// </remarks>
    public abstract class MediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of a System.Net.Http.Formatting.MediaTypeMapping with
        /// the given mediaType value.
        /// </summary>
        /// <param name="mediaType">The mediaType that is associated with the request.</param>
        protected MediaTypeMapping(string mediaType)
        {
            MediaTypeHeaderValue value = null;
            if (!MediaTypeHeaderValue.TryParse(mediaType, out value))
            {
                throw Error.ArgumentNull("mediaType");
            }

            this.MediaType = value;
        }

        /// <summary>
        ///  Gets the media type that is associated with request.
        /// </summary>
        public MediaTypeHeaderValue MediaType { get; protected set; }

        /// <summary>
        /// Returns a value indicating whether this instance can provide a
        /// <see cref="MediaTypeHeaderValue"/> for the given <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/> to check.</param>
        /// <returns>If this <paramref name="request"/>'s route data contains it returns <c>1.0</c> otherwise <c>0.0</c>.</returns>
        public abstract double TryMatchMediaType(HttpRequest request);
    }
}
