// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Contains information about the degree to which a <see cref="MediaTypeFormatter"/> matches the  
    /// explicit or implicit preferences found in an incoming request.
    /// </summary>
    internal enum ResponseFormatterSelectionResult
    {
        /// <summary>
        /// No match was found
        /// </summary>
        None,

        /// <summary>
        /// Matched on type meaning that the formatter is able to serialize the type
        /// </summary>
        MatchOnCanWriteType,

        /// <summary>
        /// Matched on explicit content-type set on the <see cref="HttpResponseMessage"/>.
        /// </summary>
        MatchOnResponseContentType,

        /// <summary>
        /// Matched on explicit accept header set in <see cref="HttpRequestMessage"/>.
        /// </summary>
        MatchOnRequestAcceptHeader,

        /// <summary>
        /// Matched on <see cref="HttpRequestMessage"/> after having applied
        /// the various <see cref="MediaTypeMapping"/>s.
        /// </summary>
        MatchOnRequestWithMediaTypeMapping,

        /// <summary>
        /// Matched on the media type of the <see cref="HttpContent"/> of the <see cref="HttpRequestMessage"/>.
        /// </summary>
        MatchOnRequestContentType,
    }
}
