// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter
{
    public class ODataMediaTypesTest
    {
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
            Assert.Equal("application/json; odata.metadata=full",
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
            Assert.Equal("application/json; odata.metadata=full; odata.streaming=false",
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
            Assert.Equal("application/json; odata.metadata=full; odata.streaming=true",
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
            Assert.Equal("application/json; odata.metadata=minimal",
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
            Assert.Equal("application/json; odata.metadata=minimal; odata.streaming=false",
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
            Assert.Equal("application/json; odata.metadata=minimal; odata.streaming=true",
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
            Assert.Equal("application/json; odata.metadata=none",
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
            Assert.Equal("application/json; odata.metadata=none; odata.streaming=false",
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
            Assert.Equal("application/json; odata.metadata=none; odata.streaming=true",
                ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrue.ToString());
        }

        [Fact]
        public void ApplicationJsonODataNoMetadataStreamingTrue_ReturnsDifferentInstances()
        {
            Assert.NotSame(ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrue,
                ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrue);
        }

        [Fact]
        public void ApplicationJsonStreamingFalse_Value()
        {
            Assert.Equal("application/json; odata.streaming=false",
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
            Assert.Equal("application/json; odata.streaming=true", ODataMediaTypes.ApplicationJsonStreamingTrue.ToString());
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

        [Theory]
        [InlineData("application/xml", ODataMetadataLevel.Default)]
        [InlineData("application/json;randomparameter=randomvalue", ODataMetadataLevel.MinimalMetadata)]
        [InlineData("application/json;odata.metadata=full", ODataMetadataLevel.FullMetadata)]
        [InlineData("application/json;odata.metadata=none", ODataMetadataLevel.NoMetadata)]
        [InlineData("application/json;odata.metadata=minimal", ODataMetadataLevel.MinimalMetadata)]
        [InlineData("application/json;odata.metadata=full", ODataMetadataLevel.FullMetadata)]
        [InlineData("application/json;odata.metadata=none", ODataMetadataLevel.NoMetadata)]
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
