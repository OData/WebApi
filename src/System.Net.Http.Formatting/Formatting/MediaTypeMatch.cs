// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http.Headers;
using System.Text;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Class that associates a <see cref="MediaTypeHeaderValue"/> with the
    /// the quality factor of the match.
    /// </summary>
    internal class MediaTypeMatch
    {
        /// <summary>
        /// Quality factor to indicate a perfect match.
        /// </summary>
        public const double Match = 1.0;

        /// <summary>
        /// Quality factor to indicate no match.
        /// </summary>
        public const double NoMatch = 0.0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTypeMatch"/> class.
        /// </summary>
        /// <param name="mediaType">The media type that has matched.</param>
        public MediaTypeMatch(MediaTypeHeaderValue mediaType)
            : this(mediaType, Match)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTypeMatch"/> class.
        /// </summary>
        /// <param name="mediaType">The media type that has matched.  A <c>null</c> value is allowed.</param>
        /// <param name="quality">The quality of the match.</param>
        public MediaTypeMatch(MediaTypeHeaderValue mediaType, double quality)
        {
            Contract.Assert(quality >= 0 && quality <= 1.0, "Quality must be from 0.0 to 1.0, inclusive.");

            // We always clone the media type because it is mutable and we do not want the original source modified.
            MediaType = mediaType == null ? null : mediaType.Clone();
            Quality = quality;
        }

        /// <summary>
        /// Gets the matched media type.
        /// </summary>
        public MediaTypeHeaderValue MediaType { get; private set; }

        /// <summary>
        /// Gets the quality of the match
        /// </summary>
        public double Quality { get; private set; }

        /// <summary>
        /// Set the character encoding on the media type. As we have already cloned the 
        /// media type upon construction of this instance this is safe to do without 
        /// any side effects.
        /// </summary>
        /// <param name="encoding">The <see cref="Encoding"/> to use.</param>
        public void SetEncoding(Encoding encoding)
        {
            if (encoding != null && MediaType != null)
            {
                MediaType.CharSet = encoding.WebName;
            }
        }
    }
}
