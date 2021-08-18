//-----------------------------------------------------------------------------
// <copyright file="ODataEnumDeserializerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Test.Abstraction;
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
            object value = deserializer.Read(ODataDeserializationTestsCommon.GetODataMessageReader(ODataDeserializationTestsCommon.GetODataMessage(content, new HttpRequestMessage(new HttpMethod("Post"), "http://localhost/OData/TestUri")), _edmModel),
                typeof(Color), readContext);

            // Assert
            Color color = Assert.IsType<Color>(value);
            Assert.Equal(Color.Blue, color);
        }

        [Fact]
        public void ReadFromStreamAsync_RawValue()
        {
            // Arrange
            string content = "{\"value\":\"Blue\"}";

            ODataEnumDeserializer deserializer = new ODataEnumDeserializer();
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _edmModel,
                ResourceType = typeof(Color)
            };

            // Act
            object value = deserializer.Read(ODataDeserializationTestsCommon.GetODataMessageReader(ODataDeserializationTestsCommon.GetODataMessage(content, new HttpRequestMessage(new HttpMethod("Post"), "http://localhost/OData/TestUri")), _edmModel),
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
            object value = deserializer.Read(ODataDeserializationTestsCommon.GetODataMessageReader(ODataDeserializationTestsCommon.GetODataMessage(content, new HttpRequestMessage(new HttpMethod("Post"), "http://localhost/OData/TestUri")), _edmModel),
                typeof(Color), readContext);

            // Assert
            EdmEnumObject color = Assert.IsType<EdmEnumObject>(value);
            Assert.NotNull(color);

            Assert.Equal("Blue", color.Value);
        }

        [Fact]
        public void ReadFromStreamAsync_ModelAlias()
        {
            // Arrange
            string content = "{\"@odata.type\":\"#NS.level\",\"value\":\"veryhigh\"}";

            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EnumType<Level>().Namespace = "NS";
            IEdmModel model = builder.GetEdmModel();

            ODataEnumDeserializer deserializer = new ODataEnumDeserializer();
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = model,
                ResourceType = typeof(Level)
            };

            // Act
            object value = deserializer.Read(ODataDeserializationTestsCommon.GetODataMessageReader(ODataDeserializationTestsCommon.GetODataMessage(content, new HttpRequestMessage(new HttpMethod("Post"), "http://localhost/OData/TestUri")), model),
                typeof(Level), readContext);

            // Assert
            Level level = Assert.IsType<Level>(value);
            Assert.Equal(Level.High, level);
        }

        public enum Color
        {
            Red,
            Blue,
            Green
        }

        [DataContract(Name = "level")]
        public enum Level
        {
            [EnumMember(Value = "low")]
            Low,
            [EnumMember(Value = "veryhigh")]
            High
        }
    }
}
