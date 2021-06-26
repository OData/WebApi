// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Formatter;
using Xunit;

namespace Microsoft.Test.OData.WebApi.AspNet.Formatter
{
    public class ODataMediaTypesTest
    {
        [Fact]
        public void ApplicationJson_Value()
        {
            Assert.Equal("application/json", ODataMediaTypes.ApplicationJson);
        }

        [Fact]
        public void ApplicationJsonODataFullMetadata_Value()
        {
            Assert.Equal("application/json;odata.metadata=full",
                ODataMediaTypes.ApplicationJsonODataFullMetadata);
        }

        [Fact]
        public void ApplicationJsonODataFullMetadataStreamingFalse_Value()
        {
            Assert.Equal("application/json;odata.metadata=full;odata.streaming=false",
                ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingFalse);
        }

        [Fact]
        public void ApplicationJsonODataFullMetadataStreamingTrue_Value()
        {
            Assert.Equal("application/json;odata.metadata=full;odata.streaming=true",
                ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingTrue);
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadata_Value()
        {
            Assert.Equal("application/json;odata.metadata=minimal",
                ODataMediaTypes.ApplicationJsonODataMinimalMetadata);
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadataStreamingFalse_Value()
        {
            Assert.Equal("application/json;odata.metadata=minimal;odata.streaming=false",
                ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingFalse);
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadataStreamingTrue_Value()
        {
            Assert.Equal("application/json;odata.metadata=minimal;odata.streaming=true",
                ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingTrue);
        }

        [Fact]
        public void ApplicationJsonODataNoMetadata_Value()
        {
            Assert.Equal("application/json;odata.metadata=none",
                ODataMediaTypes.ApplicationJsonODataNoMetadata);
        }

        [Fact]
        public void ApplicationJsonODataNoMetadataStreamingFalse_Value()
        {
            Assert.Equal("application/json;odata.metadata=none;odata.streaming=false",
                ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingFalse);
        }

        [Fact]
        public void ApplicationJsonODataNoMetadataStreamingTrue_Value()
        {
            Assert.Equal("application/json;odata.metadata=none;odata.streaming=true",
                ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrue);
        }

        [Fact]
        public void ApplicationJsonStreamingFalse_Value()
        {
            Assert.Equal("application/json;odata.streaming=false",
                ODataMediaTypes.ApplicationJsonStreamingFalse);
        }

        [Fact]
        public void ApplicationJsonStreamingTrue_Value()
        {
            Assert.Equal("application/json;odata.streaming=true", ODataMediaTypes.ApplicationJsonStreamingTrue);
        }

        [Fact]
        public void ApplicationJsonIEEE754CompatibleTrue_Value()
        {
            Assert.Equal("application/json;IEEE754Compatible=true", ODataMediaTypes.ApplicationJsonIEEE754CompatibleTrue);
        }

        [Fact]
        public void ApplicationJsonIEEE754CompatibleFalse_Value()
        {
            Assert.Equal("application/json;IEEE754Compatible=false", ODataMediaTypes.ApplicationJsonIEEE754CompatibleFalse);
        }

        [Fact]
        public void ApplicationJsonODataFullMetadataIEEE754CompatibleTrue_Value()
        {
            Assert.Equal("application/json;odata.metadata=full;IEEE754Compatible=true", ODataMediaTypes.ApplicationJsonODataFullMetadataIEEE754CompatibleTrue);
        }

        [Fact]
        public void ApplicationJsonODataFullMetadataIEEE754CompatibleFalse_Value()
        {
            Assert.Equal("application/json;odata.metadata=full;IEEE754Compatible=false", ODataMediaTypes.ApplicationJsonODataFullMetadataIEEE754CompatibleFalse);
        }

        [Fact]
        public void ApplicationJsonODataFullMetadataStreamingFalseIEEE754CompatibleTrue_Value()
        {
            Assert.Equal("application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=true", ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingFalseIEEE754CompatibleTrue);
        }

        [Fact]
        public void ApplicationJsonODataFullMetadataStreamingFalseIEEE754CompatibleFalse_Value()
        {
            Assert.Equal("application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=false", ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingFalseIEEE754CompatibleFalse);
        }

        [Fact]
        public void ApplicationJsonODataFullMetadataStreamingTrueIEEE754CompatibleTrue_Value()
        {
            Assert.Equal("application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=true", ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingTrueIEEE754CompatibleTrue);
        }

        [Fact]
        public void ApplicationJsonODataFullMetadataStreamingTrueIEEE754CompatibleFalse_Value()
        {
            Assert.Equal("application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=false", ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingTrueIEEE754CompatibleFalse);
        }

         [Fact]
        public void ApplicationJsonODataMinimalMetadataIEEE754CompatibleTrue_Value()
        {
            Assert.Equal("application/json;odata.metadata=minimal;IEEE754Compatible=true", ODataMediaTypes.ApplicationJsonODataMinimalMetadataIEEE754CompatibleTrue);
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadataIEEE754CompatibleFalse_Value()
        {
            Assert.Equal("application/json;odata.metadata=minimal;IEEE754Compatible=false", ODataMediaTypes.ApplicationJsonODataMinimalMetadataIEEE754CompatibleFalse);
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadataStreamingFalseIEEE754CompatibleTrue_Value()
        {
            Assert.Equal("application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=true", ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingFalseIEEE754CompatibleTrue);
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadataStreamingFalseIEEE754CompatibleFalse_Value()
        {
            Assert.Equal("application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=false", ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingFalseIEEE754CompatibleFalse);
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadataStreamingTrueIEEE754CompatibleTrue_Value()
        {
            Assert.Equal("application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=true", ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingTrueIEEE754CompatibleTrue);
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadataStreamingTrueIEEE754CompatibleFalse_Value()
        {
            Assert.Equal("application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false", ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingTrueIEEE754CompatibleFalse);
        }

        [Fact]
        public void ApplicationJsonODataNoMetadataIEEE754CompatibleTrue_Value()
        {
            Assert.Equal("application/json;odata.metadata=none;IEEE754Compatible=true", ODataMediaTypes.ApplicationJsonODataNoMetadataIEEE754CompatibleTrue);
        }

        [Fact]
        public void ApplicationJsonODataNoMetadataIEEE754CompatibleFalse_Value()
        {
            Assert.Equal("application/json;odata.metadata=none;IEEE754Compatible=false", ODataMediaTypes.ApplicationJsonODataNoMetadataIEEE754CompatibleFalse);
        }

        [Fact]
        public void ApplicationJsonODataNoMetadataStreamingFalseIEEE754CompatibleTrue_Value()
        {
            Assert.Equal("application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=true", ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingFalseIEEE754CompatibleTrue);
        }

        [Fact]
        public void ApplicationJsonODataNoMetadataStreamingFalseIEEE754CompatibleFalse_Value()
        {
            Assert.Equal("application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=false", ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingFalseIEEE754CompatibleFalse);
        }

        [Fact]
        public void ApplicationJsonODataNoMetadataStreamingTrueIEEE754CompatibleTrue_Value()
        {
            Assert.Equal("application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=true", ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrueIEEE754CompatibleTrue);
        }

        [Fact]
        public void ApplicationJsonODataNoMetadataStreamingTrueIEEE754CompatibleFalse_Value()
        {
            Assert.Equal("application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=false", ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrueIEEE754CompatibleFalse);
        }

        [Fact]
        public void ApplicationJsonStreamingFalseIEEE754CompatibleTrue_Value()
        {
            Assert.Equal("application/json;odata.streaming=false;IEEE754Compatible=true", ODataMediaTypes.ApplicationJsonStreamingFalseIEEE754CompatibleTrue);
        }

        [Fact]
        public void ApplicationJsonStreamingFalseIEEE754CompatibleFalse_Value()
        {
            Assert.Equal("application/json;odata.streaming=false;IEEE754Compatible=false", ODataMediaTypes.ApplicationJsonStreamingFalseIEEE754CompatibleFalse);
        }

        [Fact]
        public void ApplicationJsonStreamingTrueIEEE754CompatibleTrue_Value()
        {
            Assert.Equal("application/json;odata.streaming=true;IEEE754Compatible=true", ODataMediaTypes.ApplicationJsonStreamingTrueIEEE754CompatibleTrue);
        }

        [Fact]
        public void ApplicationJsonStreamingTrueIEEE754CompatibleFalse_Value()
        {
            Assert.Equal("application/json;odata.streaming=true;IEEE754Compatible=false", ODataMediaTypes.ApplicationJsonStreamingTrueIEEE754CompatibleFalse);
        }

        [Fact]
        public void ApplicationXml_Value()
        {
            Assert.Equal("application/xml", ODataMediaTypes.ApplicationXml);
        }

        [Theory]
        [InlineData("application/xml", ODataMetadataLevel.MinimalMetadata)]
        [InlineData("application/json;randomparameter=randomvalue", ODataMetadataLevel.MinimalMetadata)]
        [InlineData("application/json;odata.metadata=full", ODataMetadataLevel.FullMetadata)]
        [InlineData("application/json;odata.metadata=none", ODataMetadataLevel.NoMetadata)]
        [InlineData("application/json;odata.metadata=minimal", ODataMetadataLevel.MinimalMetadata)]
        [InlineData("application/json", ODataMetadataLevel.MinimalMetadata)]
        [InlineData("application/random", ODataMetadataLevel.MinimalMetadata)]
        public void GetMetadataLevel_Returns_Correct_MetadataLevel(string contentType, object metadataLevel)
        {
            MediaTypeHeaderValue contentTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);
            IEnumerable<KeyValuePair<string, string>> parameters =
                contentTypeHeaderValue.Parameters.Select(val => new KeyValuePair<string, string>(val.Name, val.Value));

            Assert.Equal(
                metadataLevel,
                ODataMediaTypes.GetMetadataLevel(contentTypeHeaderValue.MediaType, parameters));
        }
    }
}
