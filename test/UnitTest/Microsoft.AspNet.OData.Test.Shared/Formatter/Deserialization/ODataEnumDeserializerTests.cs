// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Deserialization
{
    public class ODataEnumDeserializerTests
    {
        private IEdmModel _edmModel;

        public ODataEnumDeserializerTests()
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EnumType<Color>().Namespace = "NS";
            _edmModel = builder.GetEdmModel();
        }

        [Fact]
        public void ReadFromStreamAsync()
        {
            // Arrange
            string content = "{\"@odata.type\":\"#NS.Color\",\"value\":\"Blue\"}";

            ODataEnumDeserializer deserializer = new ODataEnumDeserializer();
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _edmModel,
                ResourceType = typeof(Color)
            };

            // Act
            object value = deserializer.Read(GetODataMessageReader(GetODataMessage(content), _edmModel),
                typeof(Color), readContext);

            // Assert
            Color color = Assert.IsType<Color>(value);
            Assert.Equal(Color.Blue, color);
        }

        [Fact]
        public void ReadFromStreamAsync_ForUnType()
        {
            // Arrange
            string content = "{\"@odata.type\":\"#NS.Color\",\"value\":\"Blue\"}";

            ODataEnumDeserializer deserializer = new ODataEnumDeserializer();
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _edmModel,
                ResourceType = typeof(IEdmEnumObject)
            };

            // Act
            object value = deserializer.Read(GetODataMessageReader(GetODataMessage(content), _edmModel),
                typeof(Color), readContext);

            // Assert
            EdmEnumObject color = Assert.IsType<EdmEnumObject>(value);
            Assert.NotNull(color);

            Assert.Equal("Blue", color.Value);
        }

        private static ODataMessageReader GetODataMessageReader(IODataRequestMessage oDataRequestMessage, IEdmModel edmModel)
        {
            return new ODataMessageReader(oDataRequestMessage, new ODataMessageReaderSettings(), edmModel);
        }

        private static IODataRequestMessage GetODataMessage(string content)
        {
            // While NetCore does not use this for AspNet, it can be used here to create
            // an HttpRequestODataMessage, which is a Test type that implments IODataRequestMessage
            // wrapped around an HttpRequestMessage.
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Patch"), "http://localhost/OData/Suppliers(1)/Address");

            request.Content = new StringContent(content);
            request.Headers.Add("OData-Version", "4.0");

            MediaTypeWithQualityHeaderValue mediaType = new MediaTypeWithQualityHeaderValue("application/json");
            mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "full"));
            request.Headers.Accept.Add(mediaType);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return new HttpRequestODataMessage(request);
        }

        public enum Color
        {
            Red,
            Blue,
            Green
        }
    }
}
