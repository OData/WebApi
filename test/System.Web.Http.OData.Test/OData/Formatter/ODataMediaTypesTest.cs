// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
    }
}
