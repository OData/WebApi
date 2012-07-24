// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http.Headers;

namespace System.Net.Http.Formatting
{
    internal class ParsedMediaTypeHeaderValue
    {
        private const string MediaRangeAsterisk = "*";
        private const char MediaTypeSubtypeDelimiter = '/';

        private string _type;
        private string _subType;
        private bool? _isAllMediaRange;
        private bool? _isSubtypeMediaRange;

        public ParsedMediaTypeHeaderValue(MediaTypeHeaderValue mediaType)
        {
            Contract.Assert(mediaType != null, "The 'mediaType' parameter should not be null.");

            string[] splitMediaType = mediaType.MediaType.Split(MediaTypeSubtypeDelimiter);

            Contract.Assert(splitMediaType.Length == 2, "The constructor of the MediaTypeHeaderValue would have failed if there wasn't a type and subtype.");

            _type = splitMediaType[0];
            _subType = splitMediaType[1];
        }

        public string Type
        {
            get { return _type; }
        }

        public string Subtype
        {
            get { return _subType; }
        }

        public bool IsAllMediaRange
        {
            get
            {
                if (!_isAllMediaRange.HasValue)
                {
                    _isAllMediaRange = IsSubtypeMediaRange && String.Equals(MediaRangeAsterisk, Type, StringComparison.Ordinal);
                }
                return _isAllMediaRange.Value;
            }
        }

        public bool IsSubtypeMediaRange
        {
            get
            {
                if (!_isSubtypeMediaRange.HasValue)
                {
                    _isSubtypeMediaRange = String.Equals(MediaRangeAsterisk, Subtype, StringComparison.Ordinal);
                }
                return _isSubtypeMediaRange.Value;
            }
        }
    }
}
