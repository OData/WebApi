// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text;

namespace System.Net.Http.Formatting.Mocks
{
    public class MockContentNegotiator : DefaultContentNegotiator
    {
        public MockContentNegotiator()
        {
        }

        public MockContentNegotiator(bool excludeMatchOnTypeOnly)
            : base(excludeMatchOnTypeOnly)
        {
        }

        new public Collection<MediaTypeFormatterMatch> ComputeFormatterMatches(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
        {
            return base.ComputeFormatterMatches(type, request, formatters);
        }

        new public MediaTypeFormatterMatch SelectResponseMediaTypeFormatter(ICollection<MediaTypeFormatterMatch> matches)
        {
            return base.SelectResponseMediaTypeFormatter(matches);
        }

        new public Encoding SelectResponseCharacterEncoding(HttpRequestMessage request, MediaTypeFormatter formatter)
        {
            return base.SelectResponseCharacterEncoding(request, formatter);
        }

        new public MediaTypeFormatterMatch MatchMediaTypeMapping(HttpRequestMessage request, MediaTypeFormatter formatter)
        {
            return base.MatchMediaTypeMapping(request, formatter);
        }

        new public MediaTypeFormatterMatch MatchAcceptHeader(IEnumerable<MediaTypeWithQualityHeaderValue> sortedAcceptValues, MediaTypeFormatter formatter)
        {
            return base.MatchAcceptHeader(sortedAcceptValues, formatter);
        }

        new public MediaTypeFormatterMatch MatchRequestMediaType(HttpRequestMessage request, MediaTypeFormatter formatter)
        {
            return base.MatchRequestMediaType(request, formatter);
        }

        new public MediaTypeFormatterMatch MatchType(Type type, MediaTypeFormatter formatter)
        {
            return base.MatchType(type, formatter);
        }

        new public IEnumerable<MediaTypeWithQualityHeaderValue> SortMediaTypeWithQualityHeaderValuesByQFactor(ICollection<MediaTypeWithQualityHeaderValue> headerValues)
        {
            return base.SortMediaTypeWithQualityHeaderValuesByQFactor(headerValues);
        }

        new public IEnumerable<StringWithQualityHeaderValue> SortStringWithQualityHeaderValuesByQFactor(ICollection<StringWithQualityHeaderValue> headerValues)
        {
            return base.SortStringWithQualityHeaderValuesByQFactor(headerValues);
        }

        new public MediaTypeFormatterMatch UpdateBestMatch(MediaTypeFormatterMatch current, MediaTypeFormatterMatch potentialReplacement)
        {
            return base.UpdateBestMatch(current, potentialReplacement);
        }
    }
}
