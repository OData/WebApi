// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http.Headers;

namespace System.Net.Http.Formatting
{
    internal class ParsedMediaTypeHeaderValue
    {
        private const string MediaRangeAsterisk = "*";
        private const char MediaTypeSubTypeDelimiter = '/';

        private string _type;
        private string _subType;

        public ParsedMediaTypeHeaderValue(MediaTypeHeaderValue mediaType)
        {
            Contract.Assert(mediaType != null, "The 'mediaType' parameter should not be null.");

            string[] splitMediaType = mediaType.MediaType.Split(MediaTypeSubTypeDelimiter);

            Contract.Assert(splitMediaType.Length == 2, "The constructor of the MediaTypeHeaderValue would have failed if there wasn't a type and subtype.");

            _type = splitMediaType[0];
            _subType = splitMediaType[1];
        }

        public string Type
        {
            get { return _type; }
        }

        public string SubType
        {
            get { return _subType; }
        }

        public bool IsAllMediaRange
        {
            get { return IsSubTypeMediaRange && String.Equals(MediaRangeAsterisk, Type, StringComparison.Ordinal); }
        }

        public bool IsSubTypeMediaRange
        {
            get { return String.Equals(MediaRangeAsterisk, SubType, StringComparison.Ordinal); }
        }
    }
}
