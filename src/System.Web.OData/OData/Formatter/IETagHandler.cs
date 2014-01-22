// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace System.Web.OData.Formatter
{
    /// <summary>
    /// Exposes the ability to convert a collection of concurrency property names and values into an <see cref="EntityTagHeaderValue"/>
    /// and parse an <see cref="EntityTagHeaderValue"/> into a list of concurrency property values.
    /// </summary>
    public interface IETagHandler
    {
        /// <summary>
        /// Creates an ETag from concurrency property names and values.
        /// </summary>
        /// <param name="properties">The input property names and values.</param>
        /// <returns>The generated ETag string.</returns>
        EntityTagHeaderValue CreateETag(IDictionary<string, object> properties);

        /// <summary>
        /// Parses an ETag header value into concurrency property names and values.
        /// </summary>
        /// <param name="etagHeaderValue">The ETag header value.</param>
        /// <returns>Concurrency property names and values.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "ETag is defined in http://www.ietf.org/rfc/rfc2616.txt")]
        IDictionary<string, object> ParseETag(EntityTagHeaderValue etagHeaderValue);
    }
}
