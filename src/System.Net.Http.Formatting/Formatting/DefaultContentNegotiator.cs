// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Headers;

namespace System.Net.Http.Formatting
{
    /// <summary>
    /// Class that selects a <see cref="MediaTypeFormatter"/> for an <see cref="HttpRequestMessage"/>
    /// or <see cref="HttpResponseMessage"/>.
    /// </summary>
    public class DefaultContentNegotiator : IContentNegotiator
    {
        /// <summary>
        /// Performs content negotiating by selecting the most appropriate <see cref="MediaTypeFormatter"/> out of the passed in
        /// <paramref name="formatters"/> for the given <paramref name="request"/> that can serialize an object of the given
        /// <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to be serialized.</param>
        /// <param name="request">The request.</param>
        /// <param name="formatters">The set of <see cref="MediaTypeFormatter"/> objects from which to choose.</param>
        /// <returns>The result of the negotiation containing the most appropriate <see cref="MediaTypeFormatter"/> instance,
        /// or <c>null</c> if there is no appropriate formatter.</returns>
        public virtual ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (formatters == null)
            {
                throw new ArgumentNullException("formatters");
            }

            MediaTypeHeaderValue mediaType;
            MediaTypeFormatter formatter = RunNegotiation(type, request, formatters, out mediaType);
            if (formatter != null)
            {
                formatter = formatter.GetPerRequestFormatterInstance(type, request, mediaType);
                return new ContentNegotiationResult(formatter, mediaType);
            }
            return null;
        }

        private static MediaTypeFormatter RunNegotiation(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters, out MediaTypeHeaderValue mediaType)
        {
            // Asking to serialize a response.   This is the nominal code path.
            // We ask all formatters for their best kind of match, and then we
            // choose the best among those.
            MediaTypeFormatter formatterMatchOnType = null;
            ResponseMediaTypeMatch mediaTypeMatchOnType = null;

            MediaTypeFormatter formatterMatchOnAcceptHeader = null;
            ResponseMediaTypeMatch mediaTypeMatchOnAcceptHeader = null;

            MediaTypeFormatter formatterMatchWithMapping = null;
            ResponseMediaTypeMatch mediaTypeMatchWithMapping = null;

            MediaTypeFormatter formatterMatchOnRequestContentType = null;
            ResponseMediaTypeMatch mediaTypeMatchOnRequestContentType = null;

            foreach (MediaTypeFormatter formatter in formatters)
            {
                ResponseMediaTypeMatch match = formatter.SelectResponseMediaType(type, request);
                if (match == null)
                {
                    // Null signifies no match
                    continue;
                }

                ResponseFormatterSelectionResult matchResult = match.ResponseFormatterSelectionResult;
                switch (matchResult)
                {
                    case ResponseFormatterSelectionResult.MatchOnCanWriteType:

                        // First match by type trumps all other type matches
                        if (formatterMatchOnType == null)
                        {
                            formatterMatchOnType = formatter;
                            mediaTypeMatchOnType = match;
                        }

                        break;

                    case ResponseFormatterSelectionResult.MatchOnResponseContentType:

                        // Match on response content trumps all other choices
                        mediaType = match.MediaTypeMatch.MediaType;
                        return formatter;

                    case ResponseFormatterSelectionResult.MatchOnRequestAcceptHeader:

                        // Matches on accept headers must choose the highest quality match
                        double thisQuality = match.MediaTypeMatch.Quality;
                        if (formatterMatchOnAcceptHeader != null)
                        {
                            double bestQualitySeen = mediaTypeMatchOnAcceptHeader.MediaTypeMatch.Quality;
                            if (thisQuality <= bestQualitySeen)
                            {
                                continue;
                            }
                        }

                        formatterMatchOnAcceptHeader = formatter;
                        mediaTypeMatchOnAcceptHeader = match;

                        break;

                    case ResponseFormatterSelectionResult.MatchOnRequestWithMediaTypeMapping:

                        // Matches on accept headers using mappings must choose the highest quality match
                        double thisMappingQuality = match.MediaTypeMatch.Quality;
                        if (mediaTypeMatchWithMapping != null)
                        {
                            double bestMappingQualitySeen = mediaTypeMatchWithMapping.MediaTypeMatch.Quality;
                            if (thisMappingQuality <= bestMappingQualitySeen)
                            {
                                continue;
                            }
                        }

                        formatterMatchWithMapping = formatter;
                        mediaTypeMatchWithMapping = match;

                        break;

                    case ResponseFormatterSelectionResult.MatchOnRequestContentType:

                        // First match on request content type trumps other request content matches
                        if (formatterMatchOnRequestContentType == null)
                        {
                            formatterMatchOnRequestContentType = formatter;
                            mediaTypeMatchOnRequestContentType = match;
                        }

                        break;
                }
            }

            // If we received matches based on both supported media types and from media type mappings,
            // we want to give precedence to the media type mappings, but only if their quality is >= that of the supported media type.
            // We do this because media type mappings are the user's extensibility point and must take precedence over normal
            // supported media types in the case of a tie.   The 99% case is where both have quality 1.0.
            if (mediaTypeMatchWithMapping != null && mediaTypeMatchOnAcceptHeader != null)
            {
                if (mediaTypeMatchOnAcceptHeader.MediaTypeMatch.Quality > mediaTypeMatchWithMapping.MediaTypeMatch.Quality)
                {
                    formatterMatchWithMapping = null;
                }
            }

            // now select the formatter and media type
            // A MediaTypeMapping is highest precedence -- it is an extensibility point
            // allowing the user to override normal accept header matching
            if (formatterMatchWithMapping != null)
            {
                mediaType = mediaTypeMatchWithMapping.MediaTypeMatch.MediaType;
                return formatterMatchWithMapping;
            }
            else if (formatterMatchOnAcceptHeader != null)
            {
                mediaType = mediaTypeMatchOnAcceptHeader.MediaTypeMatch.MediaType;
                return formatterMatchOnAcceptHeader;
            }
            else if (formatterMatchOnRequestContentType != null)
            {
                mediaType = mediaTypeMatchOnRequestContentType.MediaTypeMatch.MediaType;
                return formatterMatchOnRequestContentType;
            }
            else if (formatterMatchOnType != null)
            {
                mediaType = mediaTypeMatchOnType.MediaTypeMatch.MediaType;
                return formatterMatchOnType;
            }

            mediaType = null;
            return null;
        }
    }
}
