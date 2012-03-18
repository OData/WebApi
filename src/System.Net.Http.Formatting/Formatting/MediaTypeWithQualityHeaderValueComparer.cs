using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http.Headers;

namespace System.Net.Http.Formatting
{
    /// Implementation of <see cref="IComparer{T}"/> that can compare accept media type header fields
    /// based on their quality values (a.k.a q-values). See 
    /// <see cref="StringWithQualityHeaderValueComparer"/> for a comparer for other content negotiation
    /// header field q-values.
    internal class MediaTypeWithQualityHeaderValueComparer : IComparer<MediaTypeWithQualityHeaderValue>
    {
        private static readonly MediaTypeWithQualityHeaderValueComparer _mediaTypeComparer = new MediaTypeWithQualityHeaderValueComparer();

        private MediaTypeWithQualityHeaderValueComparer()
        {
        }

        public static MediaTypeWithQualityHeaderValueComparer QualityComparer
        {
            get { return _mediaTypeComparer; }
        }

        /// <summary>
        /// Compares two <see cref="MediaTypeWithQualityHeaderValue"/> based on their quality value (a.k.a their "q-value").
        /// Values with identical q-values are considered equal (i.e the result is 0) with the exception that sub-type wild-cards are 
        /// considered less than specific media types and full wild-cards are considered less than sub-type wild-cards. This allows to 
        /// sort a sequence of <see cref="StringWithQualityHeaderValue"/> following their q-values in the order of specific media types,
        /// sub-type wildcards, and last any full wild-cards.
        /// </summary>
        /// <param name="mediaType1">The first <see cref="MediaTypeWithQualityHeaderValue"/> to compare.</param>
        /// <param name="mediaType2">The second <see cref="MediaTypeWithQualityHeaderValue"/> to compare.</param>
        /// <returns></returns>
        public int Compare(MediaTypeWithQualityHeaderValue mediaType1, MediaTypeWithQualityHeaderValue mediaType2)
        {
            Contract.Assert(mediaType1 != null, "The 'mediaType1' parameter should not be null.");
            Contract.Assert(mediaType2 != null, "The 'mediaType2' parameter should not be null.");

            ParsedMediaTypeHeaderValue parsedMediaType1 = new ParsedMediaTypeHeaderValue(mediaType1);
            ParsedMediaTypeHeaderValue parsedMediaType2 = new ParsedMediaTypeHeaderValue(mediaType2);

            int returnValue = CompareBasedOnQualityFactor(parsedMediaType1, parsedMediaType2);

            if (returnValue == 0)
            {
                if (!String.Equals(parsedMediaType1.Type, parsedMediaType2.Type, StringComparison.OrdinalIgnoreCase))
                {
                    if (parsedMediaType1.IsAllMediaRange)
                    {
                        return -1;
                    }
                    else if (parsedMediaType2.IsAllMediaRange)
                    {
                        return 1;
                    }
                }
                else if (!String.Equals(parsedMediaType1.SubType, parsedMediaType2.SubType, StringComparison.OrdinalIgnoreCase))
                {
                    if (parsedMediaType1.IsSubTypeMediaRange)
                    {
                        return -1;
                    }
                    else if (parsedMediaType2.IsSubTypeMediaRange)
                    {
                        return 1;
                    }
                }
            }

            return returnValue;
        }

        private static int CompareBasedOnQualityFactor(ParsedMediaTypeHeaderValue parsedMediaType1, ParsedMediaTypeHeaderValue parsedMediaType2)
        {
            Contract.Assert(parsedMediaType1 != null, "The 'parsedMediaType1' parameter should not be null.");
            Contract.Assert(parsedMediaType2 != null, "The 'parsedMediaType2' parameter should not be null.");

            double qualityDifference = parsedMediaType1.QualityFactor - parsedMediaType2.QualityFactor;
            if (qualityDifference < 0)
            {
                return -1;
            }
            else if (qualityDifference > 0)
            {
                return 1;
            }

            return 0;
        }
    }
}
