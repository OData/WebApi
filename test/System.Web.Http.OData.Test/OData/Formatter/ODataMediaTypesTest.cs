// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    public class ODataMediaTypesTest
    {
        [Fact]
        public void ApplicationAtomSvcXml_Value()
        {
            Assert.Equal("application/atomsvc+xml", ODataMediaTypes.ApplicationAtomSvcXml.ToString());
        }

        [Fact]
        public void ApplicationAtomSvcXml_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationAtomSvcXml, ODataMediaTypes.ApplicationAtomSvcXml);
        }

        [Fact]
        public void ApplicationAtomXml_Value()
        {
            Assert.Equal("application/atom+xml", ODataMediaTypes.ApplicationAtomXml.ToString());
        }

        [Fact]
        public void ApplicationAtomXml_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationAtomXml, ODataMediaTypes.ApplicationAtomXml);
        }

        [Fact]
        public void ApplicationAtomXmlTypeEntry_Value()
        {
            Assert.Equal("application/atom+xml; type=entry", ODataMediaTypes.ApplicationAtomXmlTypeEntry.ToString());
        }

        [Fact]
        public void ApplicationAtomXmlTypeEntry_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationAtomXmlTypeEntry, ODataMediaTypes.ApplicationAtomXmlTypeEntry);
        }

        [Fact]
        public void ApplicationAtomXmlTypeFeed_Value()
        {
            Assert.Equal("application/atom+xml; type=feed", ODataMediaTypes.ApplicationAtomXmlTypeFeed.ToString());
        }

        [Fact]
        public void ApplicationAtomXmlTypeFeed_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationAtomXmlTypeFeed, ODataMediaTypes.ApplicationAtomXmlTypeFeed);
        }

        [Fact]
        public void ApplicationJson_Value()
        {
            Assert.Equal("application/json", ODataMediaTypes.ApplicationJson.ToString());
        }

        [Fact]
        public void ApplicationJson_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationJson, ODataMediaTypes.ApplicationJson);
        }

        [Fact]
        public void ApplicationJsonODataFullMetadata_Value()
        {
            Assert.Equal("application/json; odata=fullmetadata",
                ODataMediaTypes.ApplicationJsonODataFullMetadata.ToString());
        }

        [Fact]
        public void ApplicationJsonODataFullMetadata_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationJsonODataFullMetadata,
                ODataMediaTypes.ApplicationJsonODataFullMetadata);
        }

        [Fact]
        public void ApplicationJsonODataFullMetadataStreamingFalse_Value()
        {
            Assert.Equal("application/json; odata=fullmetadata; streaming=false",
                ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingFalse.ToString());
        }

        [Fact]
        public void ApplicationJsonODataFullMetadataStreamingFalse_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingFalse,
                ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingFalse);
        }

        [Fact]
        public void ApplicationJsonODataFullMetadataStreamingTrue_Value()
        {
            Assert.Equal("application/json; odata=fullmetadata; streaming=true",
                ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingTrue.ToString());
        }

        [Fact]
        public void ApplicationJsonODataFullMetadataStreamingTrue_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingTrue,
                ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingTrue);
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadata_Value()
        {
            Assert.Equal("application/json; odata=minimalmetadata",
                ODataMediaTypes.ApplicationJsonODataMinimalMetadata.ToString());
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadata_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationJsonODataMinimalMetadata,
                ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadataStreamingFalse_Value()
        {
            Assert.Equal("application/json; odata=minimalmetadata; streaming=false",
                ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingFalse.ToString());
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadataStreamingFalse_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingFalse,
                ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingFalse);
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadataStreamingTrue_Value()
        {
            Assert.Equal("application/json; odata=minimalmetadata; streaming=true",
                ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingTrue.ToString());
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadataStreamingTrue_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingTrue,
                ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingTrue);
        }

        [Fact]
        public void ApplicationJsonODataNoMetadata_Value()
        {
            Assert.Equal("application/json; odata=nometadata",
                ODataMediaTypes.ApplicationJsonODataNoMetadata.ToString());
        }

        [Fact]
        public void ApplicationJsonODataNoMetadata_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationJsonODataNoMetadata,
                ODataMediaTypes.ApplicationJsonODataNoMetadata);
        }

        [Fact]
        public void ApplicationJsonODataNoMetadataStreamingFalse_Value()
        {
            Assert.Equal("application/json; odata=nometadata; streaming=false",
                ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingFalse.ToString());
        }

        [Fact]
        public void ApplicationJsonODataNoMetadataStreamingFalse_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingFalse,
                ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingFalse);
        }

        [Fact]
        public void ApplicationJsonODataNoMetadataStreamingTrue_Value()
        {
            Assert.Equal("application/json; odata=nometadata; streaming=true",
                ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrue.ToString());
        }

        [Fact]
        public void ApplicationJsonODataNoMetadataStreamingTrue_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrue,
                ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrue);
        }

        [Fact]
        public void ApplicationJsonODataVerbose_Value()
        {
            Assert.Equal("application/json; odata=verbose",
                ODataMediaTypes.ApplicationJsonODataVerbose.ToString());
        }

        [Fact]
        public void ApplicationJsonODataVerbose_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationJsonODataVerbose, ODataMediaTypes.ApplicationJsonODataVerbose);
        }

        [Fact]
        public void ApplicationJsonStreamingFalse_Value()
        {
            Assert.Equal("application/json; streaming=false",
                ODataMediaTypes.ApplicationJsonStreamingFalse.ToString());
        }

        [Fact]
        public void ApplicationJsonStreamingFalse_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationJsonStreamingFalse,
                ODataMediaTypes.ApplicationJsonStreamingFalse);
        }

        [Fact]
        public void ApplicationJsonStreamingTrue_Value()
        {
            Assert.Equal("application/json; streaming=true", ODataMediaTypes.ApplicationJsonStreamingTrue.ToString());
        }

        [Fact]
        public void ApplicationJsonStreamingTrue_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationJsonStreamingTrue, ODataMediaTypes.ApplicationJsonStreamingTrue);
        }

        [Fact]
        public void ApplicationXml_Value()
        {
            Assert.Equal("application/xml", ODataMediaTypes.ApplicationXml.ToString());
        }

        [Fact]
        public void ApplicationXml_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationXml, ODataMediaTypes.ApplicationXml);
        }

        [Fact]
        public void TextXml_Value()
        {
            Assert.Equal("text/xml", ODataMediaTypes.TextXml.ToString());
        }

        [Fact]
        public void TextXml_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.TextXml, ODataMediaTypes.TextXml);
        }

        [Theory]
        [InlineData("application/xml", ODataMetadataLevel.Default)]
        [InlineData("application/atom+xml", ODataMetadataLevel.Default)]
        [InlineData("application/atom+xml;odata=entry", ODataMetadataLevel.Default)]
        [InlineData("application/atom+xml;odata=feed", ODataMetadataLevel.Default)]
        [InlineData("application/atom+xml;odata=feed;randomparameter=randomvalue", ODataMetadataLevel.Default)]
        [InlineData("application/json;odata=verbose;randomparameter=randomvalue", ODataMetadataLevel.Default)]
        [InlineData("application/json;odata=fullmetadata", ODataMetadataLevel.FullMetadata)]
        [InlineData("application/json;odata=nometadata", ODataMetadataLevel.NoMetadata)]
        [InlineData("application/json;odata=minimalmetadata", ODataMetadataLevel.MinimalMetadata)]
        [InlineData("application/json;ODATA=VERBOSE", ODataMetadataLevel.Default)]
        [InlineData("application/json;ODATA=FULLMETADATA", ODataMetadataLevel.FullMetadata)]
        [InlineData("application/json;ODATA=NOMETADATA", ODataMetadataLevel.NoMetadata)]
        [InlineData("application/json;ODATA=MINIMALMETADATA", ODataMetadataLevel.MinimalMetadata)]
        [InlineData("application/json", ODataMetadataLevel.MinimalMetadata)]
        [InlineData("application/json;randomparameter=randomvalue", ODataMetadataLevel.MinimalMetadata)]
        [InlineData("application/random", ODataMetadataLevel.Default)]
        public void GetMetadataLevel_Returns_Correct_MetadataLevel(string contentType, object metadataLevel)
        {
            Assert.Equal(
                metadataLevel,
                ODataMediaTypes.GetMetadataLevel(MediaTypeHeaderValue.Parse(contentType)));
        }
    }
}
