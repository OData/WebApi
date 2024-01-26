//-----------------------------------------------------------------------------
// <copyright file="ODataMediaTypesTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
            Assert.Equal("application/json", ODataMediaTypes.ApplicationJson.ToString());
        }

        [Fact]
        public void ApplicationJsonODataFullMetadata_Value()
        {
            Assert.Equal("application/json;odata.metadata=full",
                ODataMediaTypes.ApplicationJsonODataFullMetadata.ToString());
        }

        [Fact]
        public void ApplicationJsonODataFullMetadataStreamingFalse_Value()
        {
            Assert.Equal("application/json;odata.metadata=full;odata.streaming=false",
                ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingFalse.ToString());
        }

        [Fact]
        public void ApplicationJsonODataFullMetadataStreamingTrue_Value()
        {
            Assert.Equal("application/json;odata.metadata=full;odata.streaming=true",
                ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingTrue.ToString());
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadata_Value()
        {
            Assert.Equal("application/json;odata.metadata=minimal",
                ODataMediaTypes.ApplicationJsonODataMinimalMetadata.ToString());
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadataStreamingFalse_Value()
        {
            Assert.Equal("application/json;odata.metadata=minimal;odata.streaming=false",
                ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingFalse.ToString());
        }

        [Fact]
        public void ApplicationJsonODataMinimalMetadataStreamingTrue_Value()
        {
            Assert.Equal("application/json;odata.metadata=minimal;odata.streaming=true",
                ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingTrue.ToString());
        }

        [Fact]
        public void ApplicationJsonODataNoMetadata_Value()
        {
            Assert.Equal("application/json;odata.metadata=none",
                ODataMediaTypes.ApplicationJsonODataNoMetadata.ToString());
        }

        [Fact]
        public void ApplicationJsonODataNoMetadataStreamingFalse_Value()
        {
            Assert.Equal("application/json;odata.metadata=none;odata.streaming=false",
                ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingFalse.ToString());
        }

        [Fact]
        public void ApplicationJsonODataNoMetadataStreamingTrue_Value()
        {
            Assert.Equal("application/json;odata.metadata=none;odata.streaming=true",
                ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrue.ToString());
        }

        [Fact]
        public void ApplicationJsonStreamingFalse_Value()
        {
            Assert.Equal("application/json;odata.streaming=false",
                ODataMediaTypes.ApplicationJsonStreamingFalse.ToString());
        }

        [Fact]
        public void ApplicationJsonStreamingTrue_Value()
        {
            Assert.Equal("application/json;odata.streaming=true", ODataMediaTypes.ApplicationJsonStreamingTrue.ToString());
        }

        public static IEnumerable<object[]> MediaTypesTestData = new List<object[]>
        {
            new object[] { ODataMediaTypes.ApplicationJsonIeee754CompatibleTrue, "application/json;IEEE754Compatible=true" },
            new object[] { ODataMediaTypes.ApplicationJsonIeee754CompatibleFalse, "application/json;IEEE754Compatible=false" },
            new object[] { ODataMediaTypes.ApplicationJsonODataFullMetadataIeee754CompatibleTrue, "application/json;odata.metadata=full;IEEE754Compatible=true" },
            new object[] { ODataMediaTypes.ApplicationJsonODataFullMetadataIeee754CompatibleFalse, "application/json;odata.metadata=full;IEEE754Compatible=false" },
            new object[] { ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingFalseIeee754CompatibleTrue, "application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=true" },
            new object[] { ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingFalseIeee754CompatibleFalse, "application/json;odata.metadata=full;odata.streaming=false;IEEE754Compatible=false" },
            new object[] { ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingTrueIeee754CompatibleTrue, "application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=true" },
            new object[] { ODataMediaTypes.ApplicationJsonODataFullMetadataStreamingTrueIeee754CompatibleFalse, "application/json;odata.metadata=full;odata.streaming=true;IEEE754Compatible=false" },
            new object[] { ODataMediaTypes.ApplicationJsonODataMinimalMetadataIeee754CompatibleTrue, "application/json;odata.metadata=minimal;IEEE754Compatible=true" },
            new object[] { ODataMediaTypes.ApplicationJsonODataMinimalMetadataIeee754CompatibleFalse, "application/json;odata.metadata=minimal;IEEE754Compatible=false" },
            new object[] { ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingFalseIeee754CompatibleTrue, "application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=true" },
            new object[] { ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingFalseIeee754CompatibleFalse, "application/json;odata.metadata=minimal;odata.streaming=false;IEEE754Compatible=false" },
            new object[] { ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingTrueIeee754CompatibleTrue, "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=true" },
            new object[] { ODataMediaTypes.ApplicationJsonODataMinimalMetadataStreamingTrueIeee754CompatibleFalse, "application/json;odata.metadata=minimal;odata.streaming=true;IEEE754Compatible=false" },
            new object[] { ODataMediaTypes.ApplicationJsonODataNoMetadataIeee754CompatibleTrue, "application/json;odata.metadata=none;IEEE754Compatible=true" },
            new object[] { ODataMediaTypes.ApplicationJsonODataNoMetadataIeee754CompatibleFalse, "application/json;odata.metadata=none;IEEE754Compatible=false" },
            new object[] { ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingFalseIeee754CompatibleTrue, "application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=true" },
            new object[] { ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingFalseIeee754CompatibleFalse, "application/json;odata.metadata=none;odata.streaming=false;IEEE754Compatible=false" },
            new object[] { ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrueIeee754CompatibleTrue, "application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=true" },
            new object[] { ODataMediaTypes.ApplicationJsonODataNoMetadataStreamingTrueIeee754CompatibleFalse, "application/json;odata.metadata=none;odata.streaming=true;IEEE754Compatible=false" },
            new object[] { ODataMediaTypes.ApplicationJsonStreamingFalseIeee754CompatibleTrue, "application/json;odata.streaming=false;IEEE754Compatible=true" },
            new object[] { ODataMediaTypes.ApplicationJsonStreamingFalseIeee754CompatibleFalse, "application/json;odata.streaming=false;IEEE754Compatible=false" },
            new object[] { ODataMediaTypes.ApplicationJsonStreamingTrueIeee754CompatibleTrue, "application/json;odata.streaming=true;IEEE754Compatible=true" },
            new object[] { ODataMediaTypes.ApplicationJsonStreamingTrueIeee754CompatibleFalse, "application/json;odata.streaming=true;IEEE754Compatible=false" },
        };

        [Theory]
        [MemberData(nameof(MediaTypesTestData))]
        public void VerifyMediaTypeValue(string mediaType, string expected)
        {
            Assert.Equal(mediaType, expected);
        }

        [Fact]
        public void ApplicationXml_Value()
        {
            Assert.Equal("application/xml", ODataMediaTypes.ApplicationXml.ToString());
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
