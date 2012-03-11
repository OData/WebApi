using System.Diagnostics.Contracts;
using System.Net.Http.Headers;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Extension methods for <see cref="MediaTypeHeaderValue"/>.
    /// </summary>
    internal static class MediaTypeHeaderValueExtensions
    {
        public static bool IsMediaRange(this MediaTypeHeaderValue mediaType)
        {
            Contract.Assert(mediaType != null, "The 'mediaType' parameter should not be null.");
            return new ParsedMediaTypeHeaderValue(mediaType).IsSubTypeMediaRange;
        }

        public static bool IsWithinMediaRange(this MediaTypeHeaderValue mediaType, MediaTypeHeaderValue mediaRange)
        {
            Contract.Assert(mediaType != null, "The 'mediaType' parameter should not be null.");
            Contract.Assert(mediaRange != null, "The 'mediaRange' parameter should not be null.");

            ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(mediaType);
            ParsedMediaTypeHeaderValue parsedMediaRange = new ParsedMediaTypeHeaderValue(mediaRange);

            if (!String.Equals(parsedMediaType.Type, parsedMediaRange.Type, StringComparison.OrdinalIgnoreCase))
            {
                return parsedMediaRange.IsAllMediaRange;
            }
            else if (!String.Equals(parsedMediaType.SubType, parsedMediaRange.SubType, StringComparison.OrdinalIgnoreCase))
            {
                return parsedMediaRange.IsSubTypeMediaRange;
            }

            if (!String.IsNullOrWhiteSpace(parsedMediaRange.CharSet))
            {
                return String.Equals(parsedMediaRange.CharSet, parsedMediaType.CharSet, StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }
    }
}
