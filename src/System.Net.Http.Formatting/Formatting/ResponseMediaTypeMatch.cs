// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Class that describes the media type that will be used for a response for a
    /// specific <see cref="MediaTypeFormatter"/>.
    /// </summary>
    internal class ResponseMediaTypeMatch
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseMediaTypeMatch"/> class.
        /// </summary>
        /// <param name="mediaTypeMatch">The <see cref="MediaTypeMatch"/> containing the media type and its quality factor.</param>
        /// <param name="result">The kind of match.</param>
        public ResponseMediaTypeMatch(MediaTypeMatch mediaTypeMatch, ResponseFormatterSelectionResult result)
        {
            Contract.Assert(mediaTypeMatch != null, "mediaTypeMatch cannot be null.");
            Contract.Assert(Enum.IsDefined(typeof(ResponseFormatterSelectionResult), result), "result must be valid ResponseFormatterSelectionResult.");

            ResponseFormatterSelectionResult = result;
            MediaTypeMatch = mediaTypeMatch;
        }

        /// <summary>
        /// Gets the kind of match that occurred.
        /// </summary>
        public ResponseFormatterSelectionResult ResponseFormatterSelectionResult { get; private set; }

        /// <summary>
        /// Gets the media type that was the source of the match.
        /// </summary>
        public MediaTypeMatch MediaTypeMatch { get; private set; }
    }
}
