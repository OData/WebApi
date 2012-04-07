// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http.Headers;

namespace System.Net.Http.Formatting
{
    internal class ParsedMediaTypeHeaderValue
    {
        private const string MediaRangeAsterisk = "*";
        private const char MediaTypeSubTypeDelimiter = '/';
        private const string QualityFactorParameterName = "q";
        private const double DefaultQualityFactor = 1.0;

        private MediaTypeHeaderValue _mediaType;
        private string _type;
        private string _subType;
        private bool? _hasNonQualityFactorParameter;
        private double? _qualityFactor;

        public ParsedMediaTypeHeaderValue(MediaTypeHeaderValue mediaType)
        {
            Contract.Assert(mediaType != null, "The 'mediaType' parameter should not be null.");

            _mediaType = mediaType;
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

        public bool HasNonQualityFactorParameter
        {
            get
            {
                if (!_hasNonQualityFactorParameter.HasValue)
                {
                    _hasNonQualityFactorParameter = false;
                    foreach (NameValueHeaderValue param in _mediaType.Parameters)
                    {
                        if (!String.Equals(QualityFactorParameterName, param.Name, StringComparison.Ordinal))
                        {
                            _hasNonQualityFactorParameter = true;
                        }
                    }
                }

                return _hasNonQualityFactorParameter.Value;
            }
        }

        public string CharSet
        {
            get { return _mediaType.CharSet; }
        }

        public double QualityFactor
        {
            get
            {
                if (!_qualityFactor.HasValue)
                {
                    MediaTypeWithQualityHeaderValue mediaTypeWithQuality = _mediaType as MediaTypeWithQualityHeaderValue;
                    if (mediaTypeWithQuality != null)
                    {
                        _qualityFactor = mediaTypeWithQuality.Quality;
                    }

                    if (!_qualityFactor.HasValue)
                    {
                        _qualityFactor = DefaultQualityFactor;
                    }
                }

                return _qualityFactor.Value;
            }
        }
    }
}
