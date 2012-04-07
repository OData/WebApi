// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Headers;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Class that provides <see cref="MediaTypeHeaderValue"/>s for a request or response
    /// from a media range.
    /// </summary>
    public class MediaRangeMapping : MediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaRangeMapping"/> class.
        /// </summary>
        /// <param name="mediaRange">The <see cref="MediaTypeHeaderValue"/> that provides a description
        /// of the media range.</param>
        /// <param name="mediaType">The <see cref="MediaTypeHeaderValue"/> to return on a match.</param>
        public MediaRangeMapping(MediaTypeHeaderValue mediaRange, MediaTypeHeaderValue mediaType)
            : base(mediaType)
        {
            Initialize(mediaRange);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaRangeMapping"/> class.
        /// </summary>
        /// <param name="mediaRange">The description of the media range.</param>
        /// <param name="mediaType">The media type to return on a match.</param>
        public MediaRangeMapping(string mediaRange, string mediaType)
            : base(mediaType)
        {
            if (String.IsNullOrWhiteSpace(mediaRange))
            {
                throw new ArgumentNullException("mediaRange");
            }

            Initialize(new MediaTypeHeaderValue(mediaRange));
        }

        /// <summary>
        /// Gets the <see cref="MediaTypeHeaderValue"/>
        /// describing the known media range.
        /// </summary>
        public MediaTypeHeaderValue MediaRange { get; private set; }

        /// <summary>
        /// Returns a value indicating whether this <see cref="MediaRangeMapping"/>
        /// instance can provide a <see cref="MediaTypeHeaderValue"/> for the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to check.</param>
        /// <returns>If this instance can match <paramref name="request"/>
        /// it returns the quality of the match otherwise <c>0.0</c>.</returns>
        public override double TryMatchMediaType(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            ICollection<MediaTypeWithQualityHeaderValue> acceptHeader = request.Headers.Accept;
            if (acceptHeader != null)
            {
                foreach (MediaTypeWithQualityHeaderValue mediaType in acceptHeader)
                {
                    if (MediaRange.IsSubsetOf(mediaType))
                    {
                        return mediaType.Quality.HasValue ? mediaType.Quality.Value : MediaTypeMatch.Match;
                    }
                }
            }

            return MediaTypeMatch.NoMatch;
        }

        private void Initialize(MediaTypeHeaderValue mediaRange)
        {
            if (mediaRange == null)
            {
                throw new ArgumentNullException("mediaRange");
            }

            if (!mediaRange.IsMediaRange())
            {
                throw new InvalidOperationException(RS.Format(Properties.Resources.InvalidMediaRange, mediaRange.ToString()));
            }

            MediaRange = mediaRange;
        }
    }
}
