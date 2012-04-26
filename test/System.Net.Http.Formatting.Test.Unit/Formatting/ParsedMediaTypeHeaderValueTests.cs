// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class ParsedMediaTypeHeadeValueTests
    {
        [Fact]
        [Trait("Description", "MediaTypeHeaderValue ensures only valid media types are constructed.")]
        public void MediaTypeHeaderValue_Ensures_Valid_MediaType()
        {
            string[] invalidMediaTypes = new string[] { "", " ", "\n", "\t", "text", "text/", "text\\", "\\", "//", "text/[", "text/ ", " text/", " text/ ", "text\\ ", " text\\", " text\\ ", "text\\xml", "text//xml" };

            foreach (string invalidMediaType in invalidMediaTypes)
            {
                Assert.Throws<Exception>(() => new MediaTypeHeaderValue(invalidMediaType), exceptionMessage: null, allowDerivedExceptions: true);
            }
        }

        [Fact]
        [Trait("Description", "ParsedMediaTypeHeadeValue.Type returns the media type.")]
        public void Type_Returns_Just_The_Type()
        {
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("text/xml");
            ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(mediaType);
            Assert.Equal("text", parsedMediaType.Type);

            mediaType = new MediaTypeHeaderValue("text/*");
            parsedMediaType = new ParsedMediaTypeHeaderValue(mediaType);
            Assert.Equal("text", parsedMediaType.Type);

            mediaType = new MediaTypeHeaderValue("*/*");
            parsedMediaType = new ParsedMediaTypeHeaderValue(mediaType);
            Assert.Equal("*", parsedMediaType.Type);
        }

        [Fact]
        [Trait("Description", "ParsedMediaTypeHeadeValue.SubType returns the media sub-type.")]
        public void SubType_Returns_Just_The_Sub_Type()
        {
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("text/xml");
            ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(mediaType);
            Assert.Equal("xml", parsedMediaType.SubType);

            mediaType = new MediaTypeHeaderValue("text/*");
            parsedMediaType = new ParsedMediaTypeHeaderValue(mediaType);
            Assert.Equal("*", parsedMediaType.SubType);

            mediaType = new MediaTypeHeaderValue("*/*");
            parsedMediaType = new ParsedMediaTypeHeaderValue(mediaType);
            Assert.Equal("*", parsedMediaType.SubType);
        }

        [Fact]
        [Trait("Description", "ParsedMediaTypeHeadeValue.IsSubTypeMediaRange returns true for media ranges.")]
        public void IsSubTypeMediaRange_Returns_True_For_Media_Ranges()
        {
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("text/*");
            ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(mediaType);
            Assert.True(parsedMediaType.IsSubTypeMediaRange, "ParsedMediaTypeHeadeValue.IsSubTypeMediaRange should have returned true.");

            mediaType = new MediaTypeHeaderValue("*/*");
            parsedMediaType = new ParsedMediaTypeHeaderValue(mediaType);
            Assert.True(parsedMediaType.IsSubTypeMediaRange, "ParsedMediaTypeHeadeValue.IsSubTypeMediaRange should have returned true.");
        }

        [Fact]
        [Trait("Description", "ParsedMediaTypeHeadeValue.IsAllMediaRange returns true only when both the type and subtype are wildcard characters.")]
        public void IsAllMediaRange_Returns_True_Only_When_Type_And_SubType_Are_Wildcards()
        {
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("text/*");
            ParsedMediaTypeHeaderValue parsedMediaType = new ParsedMediaTypeHeaderValue(mediaType);
            Assert.False(parsedMediaType.IsAllMediaRange, "ParsedMediaTypeHeadeValue.IsAllMediaRange should have returned false.");

            mediaType = new MediaTypeHeaderValue("*/*");
            parsedMediaType = new ParsedMediaTypeHeaderValue(mediaType);
            Assert.True(parsedMediaType.IsAllMediaRange, "ParsedMediaTypeHeadeValue.IsAllMediaRange should have returned true.");

            mediaType = new MediaTypeHeaderValue("*/xml");
            parsedMediaType = new ParsedMediaTypeHeaderValue(mediaType);
            Assert.False(parsedMediaType.IsAllMediaRange, "ParsedMediaTypeHeadeValue.IsAllMediaRange should have returned false.");
        }
    }
}
