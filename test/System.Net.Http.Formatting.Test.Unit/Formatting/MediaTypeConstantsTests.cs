// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.TestCommon;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http
{
    public class MediaTypeConstantsTests
    {
        [Fact]
        [Trait("Description", "Class is internal static type.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(MediaTypeConstants), TypeAssert.TypeProperties.IsClass | TypeAssert.TypeProperties.IsStatic);
        }


        private static void ValidateClones(MediaTypeHeaderValue clone1, MediaTypeHeaderValue clone2, string charset)
        {
            Assert.NotNull(clone1);
            Assert.NotNull(clone2);
            Assert.NotSame(clone1, clone2);
            Assert.Equal(clone1.MediaType, clone2.MediaType);
            Assert.Equal(charset, clone1.CharSet);
            Assert.Equal(charset, clone2.CharSet);
        }

        [Fact]
        [Trait("Description", "HtmlMediaType returns clone")]
        public void HtmlMediaTypeReturnsClone()
        {
            ValidateClones(MediaTypeConstants.HtmlMediaType, MediaTypeConstants.HtmlMediaType, Encoding.UTF8.WebName);
        }

        [Fact]
        [Trait("Description", "ApplicationXmlMediaType returns clone")]
        public void ApplicationXmlMediaTypeReturnsClone()
        {
            ValidateClones(MediaTypeConstants.ApplicationXmlMediaType, MediaTypeConstants.ApplicationXmlMediaType, null);
        }

        [Fact]
        [Trait("Description", "ApplicationJsonMediaType returns clone")]
        public void ApplicationJsonMediaTypeReturnsClone()
        {
            ValidateClones(MediaTypeConstants.ApplicationJsonMediaType, MediaTypeConstants.ApplicationJsonMediaType, null);
        }

        [Fact]
        [Trait("Description", "TextXmlMediaType returns clone")]
        public void TextXmlMediaTypeReturnsClone()
        {
            ValidateClones(MediaTypeConstants.TextXmlMediaType, MediaTypeConstants.TextXmlMediaType, null);
        }

        [Fact]
        [Trait("Description", "TextJsonMediaType returns clone")]
        public void TextJsonMediaTypeReturnsClone()
        {
            ValidateClones(MediaTypeConstants.TextJsonMediaType, MediaTypeConstants.TextJsonMediaType, null);
        }

        [Fact]
        [Trait("Description", "ApplicationFormUrlEncodedMediaType returns clone")]
        public void ApplicationFormUrlEncodedMediaTypeReturnsClone()
        {
            ValidateClones(MediaTypeConstants.ApplicationFormUrlEncodedMediaType, MediaTypeConstants.ApplicationFormUrlEncodedMediaType, null);
        }


    }
}
