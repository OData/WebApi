// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.OData.Formatter.Serialization.Models;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataCollectionDeserializerTests
    {
        private static IEdmModel _model = GetEdmModel();
        private static ODataDeserializerProvider _deserializerProvider = new DefaultODataDeserializerProvider();
        private static IEdmTypeReference _addressType = _model.GetEdmTypeReference(typeof(Address)).AsComplex();
        private static IEdmCollectionTypeReference _addressCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(_addressType), isNullable: false);
        private static IEdmCollectionTypeReference _intCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(_model.GetEdmTypeReference(typeof(int))), isNullable: false);

        [Fact]
        public void Ctor_ThrowsArgumentNull_DeserializerProvider()
        {
            IEdmCollectionTypeReference collectionType = new Mock<IEdmCollectionTypeReference>().Object;
            Assert.ThrowsArgumentNull(
                () => new ODataCollectionDeserializer(deserializerProvider: null),
                "deserializerProvider");
        }

        [Fact]
        public void Read_ThrowsArgumentNull_MessageReader()
        {
            var deserializer = new ODataCollectionDeserializer(new DefaultODataDeserializerProvider());

            Assert.ThrowsArgumentNull(
                () => deserializer.Read(messageReader: null, type: typeof(int[]), readContext: new ODataDeserializerContext()),
                "messageReader");
        }

        [Fact]
        public void Read_ThrowsArgumentMustBeOfType_Type()
        {
            var deserializer = new ODataCollectionDeserializer(new DefaultODataDeserializerProvider());

            Assert.ThrowsArgument(
                () => deserializer.Read(messageReader: ODataTestUtil.GetMockODataMessageReader(), type: typeof(int),
                    readContext: new ODataDeserializerContext { Model = _model }),
                "type", "The argument must be of type 'Collection'.");
        }

        [Fact]
        public void Read_ReturnsEdmComplexObjectCollection_TypelessMode()
        {
            // Arrange
            HttpContent content = new StringContent("{ 'value': [ { 'City' : 'Redmond' } ] }");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            IODataRequestMessage request = new ODataMessageWrapper(content.ReadAsStreamAsync().Result, content.Headers);
            ODataMessageReader reader = new ODataMessageReader(request, new ODataMessageReaderSettings(), _model);
            var deserializer = new ODataCollectionDeserializer(new DefaultODataDeserializerProvider());
            ODataDeserializerContext readContext = new ODataDeserializerContext
            {
                Model = _model,
                ResourceType = typeof(IEdmObject),
                ResourceEdmType = _addressCollectionType
            };

            // Act
            var result = deserializer.Read(reader, typeof(IEdmObject), readContext);

            // Assert
            IEdmObject edmObject = Assert.IsType<EdmComplexObjectCollection>(result);
            Assert.Equal(_addressCollectionType, edmObject.GetEdmType());
        }

        [Fact]
        public void ReadInline_ThrowsArgument_ArgumentMustBeOfType()
        {
            var deserializer = new ODataCollectionDeserializer(new DefaultODataDeserializerProvider());

            Assert.Throws<ArgumentException>(
                () => deserializer.ReadInline(42, _intCollectionType, new ODataDeserializerContext()),
                "The argument must be of type 'ODataCollectionValue'.\r\nParameter name: item");
        }

        [Fact]
        public void ReadInline_ThrowsArgumentNull_EdmType()
        {
            var deserializer = new ODataCollectionDeserializer(new DefaultODataDeserializerProvider());

            Assert.ThrowsArgumentNull(
                () => deserializer.ReadInline(42, null, new ODataDeserializerContext()),
                "edmType");
        }

        [Fact]
        public void ReadInline_ReturnsNull_IfItemIsNull()
        {
            var deserializer = new ODataCollectionDeserializer(new DefaultODataDeserializerProvider());
            Assert.Null(deserializer.ReadInline(item: null, edmType: _intCollectionType, readContext: new ODataDeserializerContext()));
        }

        [Fact]
        public void ReadInline_Calls_ReadCollectionValue()
        {
            // Arrange
            Mock<ODataCollectionDeserializer> deserializer = new Mock<ODataCollectionDeserializer>(new DefaultODataDeserializerProvider());
            ODataCollectionValue collectionValue = new ODataCollectionValue();
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializer.CallBase = true;
            deserializer.Setup(s => s.ReadCollectionValue(collectionValue, _intCollectionType.ElementType(), readContext)).Verifiable();

            // Act
            deserializer.Object.ReadInline(collectionValue, _intCollectionType, readContext);

            // Assert
            deserializer.Verify();
        }

        [Fact]
        public void ReadCollectionValue_ThrowsArgumentNull_CollectionValue()
        {
            var deserializer = new ODataCollectionDeserializer(new DefaultODataDeserializerProvider());

            Assert.ThrowsArgumentNull(
                () => deserializer.ReadCollectionValue(collectionValue: null, elementType: _intCollectionType.ElementType(),
                    readContext: new ODataDeserializerContext()).GetEnumerator().MoveNext(),
                "collectionValue");
        }

        [Fact]
        public void ReadCollectionValue_ThrowsArgumentNull_ElementType()
        {
            var deserializer = new ODataCollectionDeserializer(new DefaultODataDeserializerProvider());

            Assert.ThrowsArgumentNull(
                () => deserializer.ReadCollectionValue(new ODataCollectionValue(), elementType: null,
                    readContext: new ODataDeserializerContext()).GetEnumerator().MoveNext(),
                "elementType");
        }

        [Fact]
        public void ReadCollectionValue_Throws_IfElementTypeCannotBeDeserialized()
        {
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(_addressType)).Returns<ODataEntityDeserializer>(null);
            var deserializer = new ODataCollectionDeserializer(deserializerProvider.Object);

            Assert.Throws<SerializationException>(
                () => deserializer.ReadCollectionValue(new ODataCollectionValue() { Items = new[] { 1, 2, 3 } },
                    _addressCollectionType.ElementType(), new ODataDeserializerContext())
                    .GetEnumerator()
                    .MoveNext(),
                "'System.Web.Http.OData.Formatter.Serialization.Models.Address' cannot be deserialized using the ODataMediaTypeFormatter.");
        }

        [Fact]
        public void Read_Roundtrip_ComplexCollection()
        {
            Address[] addresses = new[]
                {
                    new Address { City ="Redmond", ZipCode ="1", Street ="A", State ="123"},
                    new Address { City ="Seattle", ZipCode ="2", Street ="S", State ="321"}
                };
            ODataCollectionSerializer serializer = new ODataCollectionSerializer(new DefaultODataSerializerProvider());
            ODataCollectionDeserializer deserializer = new ODataCollectionDeserializer(_deserializerProvider);

            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);
            ODataMessageWriter messageWriter = new ODataMessageWriter(message as IODataResponseMessage, new ODataMessageWriterSettings(), _model);
            ODataMessageReader messageReader = new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), _model);
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "Property", Model = _model };
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = _model };

            serializer.WriteObject(addresses, addresses.GetType(), messageWriter, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            IEnumerable readAddresses = deserializer.Read(messageReader, typeof(Address[]), readContext) as IEnumerable;

            Assert.Equal(addresses, readAddresses.Cast<Address>(), new AddressComparer());
        }

        [Fact]
        public void Read_Roundtrip_PrimitiveCollection()
        {
            int[] numbers = Enumerable.Range(0, 100).ToArray();

            ODataCollectionSerializer serializer = new ODataCollectionSerializer(new DefaultODataSerializerProvider());
            ODataCollectionDeserializer deserializer = new ODataCollectionDeserializer(_deserializerProvider);

            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);
            ODataMessageWriter messageWriter = new ODataMessageWriter(message as IODataResponseMessage, new ODataMessageWriterSettings(), _model);
            ODataMessageReader messageReader = new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), _model);
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "Property", Model = _model };
            ODataDeserializerContext readContext = new ODataDeserializerContext() { Model = _model };

            serializer.WriteObject(numbers, numbers.GetType(), messageWriter, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            IEnumerable readnumbers = deserializer.Read(messageReader, typeof(int[]), readContext) as IEnumerable;

            Assert.Equal(numbers, readnumbers.Cast<int>());
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.ComplexType<Address>();
            return builder.GetEdmModel();
        }

        private class AddressComparer : IEqualityComparer<Address>
        {
            public bool Equals(Address x, Address y)
            {
                return x.City == y.City && x.Country == y.Country && x.State == y.State && x.Street == y.Street && x.ZipCode == y.ZipCode;
            }

            public int GetHashCode(Address obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
