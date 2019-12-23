// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Deserialization
{
    public class ODataCollectionDeserializerTests
    {
        private static readonly IEdmModel Model = GetEdmModel();

        private static readonly ODataSerializerProvider SerializerProvider = ODataSerializerProviderFactory.Create();

        private static readonly ODataDeserializerProvider DeserializerProvider = ODataDeserializerProviderFactory.Create();

        private static readonly IEdmEnumTypeReference ColorType =
            new EdmEnumTypeReference(Model.SchemaElements.OfType<IEdmEnumType>().First(c => c.Name == "Color"),
                isNullable: true);

        private static readonly IEdmCollectionTypeReference ColorCollectionType = new EdmCollectionTypeReference(new EdmCollectionType((ColorType)));

        private static readonly IEdmCollectionTypeReference IntCollectionType =
            new EdmCollectionTypeReference(new EdmCollectionType(Model.GetEdmTypeReference(typeof(int))));

        [Fact]
        public void Ctor_ThrowsArgumentNull_DeserializerProvider()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ODataCollectionDeserializer(deserializerProvider: null),
                "deserializerProvider");
        }

        [Fact]
        public void Read_ThrowsArgumentNull_MessageReader()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => deserializer.Read(messageReader: null, type: typeof(int[]), readContext: new ODataDeserializerContext()),
                "messageReader");
        }

        [Fact]
        public void Read_ThrowsArgumentMustBeOfType_Type()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(() => deserializer.Read(messageReader: ODataTestUtil.GetMockODataMessageReader(),
                type: typeof(int), readContext: new ODataDeserializerContext { Model = Model }),
                "type", "The argument must be of type 'Collection'.");
        }

        [Fact]
        public void ReadInline_ThrowsArgument_ArgumentMustBeOfType()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            // Act & Assert
#if NETCOREAPP3_1
            ExceptionAssert.Throws<ArgumentException>(
                () => deserializer.ReadInline(42, IntCollectionType, new ODataDeserializerContext()),
                "The argument must be of type 'ODataCollectionValue'. (Parameter 'item')");
#else

            ExceptionAssert.Throws<ArgumentException>(
                () => deserializer.ReadInline(42, IntCollectionType, new ODataDeserializerContext()),
                "The argument must be of type 'ODataCollectionValue'.\r\nParameter name: item");
#endif
        }

        [Fact]
        public void ReadInline_ThrowsArgumentNull_EdmType()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => deserializer.ReadInline(42, null, new ODataDeserializerContext()),
                "edmType");
        }

        [Fact]
        public void ReadInline_ReturnsNull_IfItemIsNull()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            // Act & Assert
            Assert.Null(deserializer.ReadInline(item: null, edmType: IntCollectionType, readContext: new ODataDeserializerContext()));
        }

        [Fact]
        public void ReadInline_Calls_ReadCollectionValue()
        {
            // Arrange
            Mock<ODataCollectionDeserializer> deserializer = new Mock<ODataCollectionDeserializer>(DeserializerProvider);
            ODataCollectionValue collectionValue = new ODataCollectionValue();
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializer.CallBase = true;
            deserializer.Setup(s => s.ReadCollectionValue(collectionValue, IntCollectionType.ElementType(), readContext)).Verifiable();

            // Act
            deserializer.Object.ReadInline(collectionValue, IntCollectionType, readContext);

            // Assert
            deserializer.Verify();
        }

        [Fact]
        public void ReadCollectionValue_ThrowsArgumentNull_CollectionValue()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => deserializer.ReadCollectionValue(collectionValue: null,
                elementType: IntCollectionType.ElementType(), readContext: new ODataDeserializerContext()).GetEnumerator().MoveNext(),
                "collectionValue");
        }

        [Fact]
        public void ReadCollectionValue_ThrowsArgumentNull_ElementType()
        {
            // Arrange
            var deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => deserializer.ReadCollectionValue(new ODataCollectionValue(), elementType: null,
                    readContext: new ODataDeserializerContext()).GetEnumerator().MoveNext(),
                "elementType");
        }

        [Fact]
        public void ReadCollectionValue_Throws_IfElementTypeCannotBeDeserialized()
        {
            // Arrange
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(ColorType)).Returns<ODataResourceDeserializer>(null);
            var deserializer = new ODataCollectionDeserializer(deserializerProvider.Object);

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => deserializer.ReadCollectionValue(new ODataCollectionValue() { Items = new[] { 1, 2, 3 }.Cast<object>() },
                    ColorCollectionType.ElementType(), new ODataDeserializerContext())
                    .GetEnumerator()
                    .MoveNext(),
                "'NS.Color' cannot be deserialized using the ODataMediaTypeFormatter.");
        }

        [Fact]
        public void Read_Roundtrip_PrimitiveCollection()
        {
            // Arrange
            int[] numbers = Enumerable.Range(0, 100).ToArray();

            ODataCollectionSerializer serializer = new ODataCollectionSerializer(SerializerProvider);
            ODataCollectionDeserializer deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/") }
            };
            ODataMessageWriter messageWriter = new ODataMessageWriter(message as IODataResponseMessage, settings, Model);
            ODataMessageReader messageReader = new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), Model);
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "Property", Model = Model };
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = Model };

            // Act
            serializer.WriteObject(numbers, numbers.GetType(), messageWriter, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            IEnumerable readnumbers = deserializer.Read(messageReader, typeof(int[]), readContext) as IEnumerable;

            // Assert
            Assert.Equal(numbers, readnumbers.Cast<int>());
        }

        [Fact]
        public void Read_Roundtrip_EnumCollection()
        {
            // Arrange
            Color[] colors = {Color.Blue, Color.Green};

            ODataCollectionSerializer serializer = new ODataCollectionSerializer(SerializerProvider);
            ODataCollectionDeserializer deserializer = new ODataCollectionDeserializer(DeserializerProvider);

            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/") }
            };
            ODataMessageWriter messageWriter = new ODataMessageWriter(message as IODataResponseMessage, settings, Model);
            ODataMessageReader messageReader = new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), Model);
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "Property", Model = Model };
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = Model };

            // Act
            serializer.WriteObject(colors, colors.GetType(), messageWriter, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            IEnumerable readAddresses = deserializer.Read(messageReader, typeof(Color[]), readContext) as IEnumerable;

            // Assert
            Assert.Equal(colors, readAddresses.Cast<Color>());
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EnumType<Color>().Namespace = "NS";
            return builder.GetEdmModel();
        }

        public enum Color
        {
            Red,
            Blue,
            Green
        }
    }
}
