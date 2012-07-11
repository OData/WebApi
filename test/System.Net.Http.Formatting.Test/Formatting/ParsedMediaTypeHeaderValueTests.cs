// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Microsoft.TestCommon;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class ParsedMediaTypeHeadeValueTests
    {
        public static TheoryDataSet<string> FullMediaRanges
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "*/*",
                    "*/*; charset=utf-8",
                    "*/*; charset=utf-8; q=1.0",
                };
            }
        }

        public static TheoryDataSet<string> SubTypeMediaRanges
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "text/*",
                    "TEXT/*",
                    "application/*; charset=utf-8",
                    "APPLICATION/*; charset=utf-8",
                    "APPLICATION/*; charset=utf-8; q=1.0",
                };
            }
        }

        public static TheoryDataSet<string> InvalidMediaRanges
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "*/text",
                    "*/XML",
                };
            }
        }
        public static TheoryDataSet<string> NonMediaRanges
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "text/plain",
                    "TEXT/XML",
                    "application/xml; charset=utf-8",
                    "APPLICATION/xml; charset=utf-8",
                };
            }
        }

        public static TheoryDataSet<string> InvalidNonMediaRanges
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "",
                    " ",
                    "\n",
                    "\t",
                    "text",
                    "text/",
                    "text\\", 
                    "\\", "//", 
                    "text/[", 
                    "text/ ", 
                    " text/", 
                    " text/ ", 
                    "text\\ ", 
                    " text\\", 
                    " text\\ ", 
                    "text\\xml", 
                    "text//xml" 
                };
            }
        }

        [Theory]
        [PropertyData("InvalidNonMediaRanges")]
        public void MediaTypeHeaderValue_EnsuresValidMediaType(string invalidMediaType)
        {
            Assert.Throws<Exception>(() => new MediaTypeHeaderValue(invalidMediaType), exceptionMessage: null, allowDerivedExceptions: true);
        }

        [Theory]
        [PropertyData("FullMediaRanges")]
        [PropertyData("SubTypeMediaRanges")]
        [PropertyData("InvalidMediaRanges")]
        [PropertyData("NonMediaRanges")]
        public void Type_ReturnsJustTheType(string mediaType)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(mediaType);
            string type = mediaTypeHeaderValue.MediaType.Split('/')[0];
            ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(mediaTypeHeaderValue);
            Assert.Equal(type, parsedMediaType.Type);
        }

        [Theory]
        [PropertyData("FullMediaRanges")]
        [PropertyData("SubTypeMediaRanges")]
        [PropertyData("InvalidMediaRanges")]
        [PropertyData("NonMediaRanges")]
        public void SubType_ReturnsJustTheSubType(string mediaType)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(mediaType);
            string subtype = mediaTypeHeaderValue.MediaType.Split('/')[1];
            ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(mediaTypeHeaderValue);
            Assert.Equal(subtype, parsedMediaType.Subtype);
        }

        [Theory]
        [PropertyData("FullMediaRanges")]
        [PropertyData("SubTypeMediaRanges")]
        public void IsSubTypeMediaRange_ReturnsTrueForSubTypeMediaRanges(string mediaType)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(mediaType);
            ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(mediaTypeHeaderValue);
            Assert.True(parsedMediaType.IsSubtypeMediaRange);
        }

        [Theory]
        [PropertyData("InvalidMediaRanges")]
        [PropertyData("NonMediaRanges")]
        public void IsSubTypeMediaRange_ReturnsFalseForNonSubTypeMediaRanges(string mediaType)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(mediaType);
            ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(mediaTypeHeaderValue);
            Assert.False(parsedMediaType.IsSubtypeMediaRange);
        }

        [Theory]
        [PropertyData("FullMediaRanges")]
        public void IsAllMediaRange_ReturnsTrueForFullMediaTypeRanges(string mediaType)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(mediaType);
            ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(mediaTypeHeaderValue);
            Assert.True(parsedMediaType.IsAllMediaRange);
        }

        [Theory]
        [PropertyData("SubTypeMediaRanges")]
        [PropertyData("InvalidMediaRanges")]
        [PropertyData("NonMediaRanges")]
        public void IsAllMediaRange_ReturnsFalseForNonFullMediaTypeRanges(string mediaType)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(mediaType);
            ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(mediaTypeHeaderValue);
            Assert.False(parsedMediaType.IsAllMediaRange);
        }
    }
}
