// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Linq;
using System.IO;
using System.Web.Http.OData.Formatter.Serialization;
using System.Xml.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Microsoft.TestCommon.Types;
using Moq;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ODataPrimitiveDeserializerTests
    {
        public static TheoryDataSet<object, object> NonEdmPrimitiveData
        {
            get
            {
                return new TheoryDataSet<object, object>
                {
                    { null, null },
                    { (char)'1', "1" },
                    { (char[]) new char[] {'1'}, "1" },
                    { (UInt16)1, (int)1 },
                    { (UInt32)1, (long)1 },
                    { (UInt64)1, (long)1 },
                    //(Stream) new MemoryStream(new byte[] { 1 }), // TODO: Enable once we have support for streams
                    { new XElement(XName.Get("element","namespace")), new XElement(XName.Get("element","namespace")).ToString() },
                    { new Binary(new byte[] {1}), new byte[] {1} },

                    //Enums
                    { SimpleEnum.Second, "Second" },
                    { LongEnum.ThirdLong, "ThirdLong" },
                    { FlagsEnum.One | FlagsEnum.Four, "One, Four" }
                };
            }
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmType()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataPrimitiveDeserializer(edmType: null),
                "edmType");
        }

        [Fact]
        public void Ctor_SetsProperty_PrimitiveType()
        {
            Mock<IEdmPrimitiveTypeReference> primitiveType = new Mock<IEdmPrimitiveTypeReference>();
            var deserializer = new ODataPrimitiveDeserializer(primitiveType.Object);

            Assert.Equal(primitiveType.Object, deserializer.PrimitiveType);
        }

        [Fact]
        public void ReadInline_ReturnsNull_IfItemIsNull()
        {
            IEdmPrimitiveTypeReference primitiveType = EdmCoreModel.Instance.GetInt32(isNullable: true);
            var deserializer = new ODataPrimitiveDeserializer(primitiveType);

            Assert.Null(deserializer.ReadInline(item: null, readContext: new ODataDeserializerContext()));
        }

        [Fact]
        public void ReadInline_Throws_ArgumentMustBeOfType()
        {
            IEdmPrimitiveTypeReference primitiveType = EdmCoreModel.Instance.GetInt32(isNullable: true);
            var deserializer = new ODataPrimitiveDeserializer(primitiveType);

            Assert.ThrowsArgument(
                () => deserializer.ReadInline(42, new ODataDeserializerContext()),
                "item",
                "The argument must be of type 'ODataProperty'");
        }

        [Fact]
        public void ReadInline_Calls_ReadPrimitive()
        {
            // Arrange
            IEdmPrimitiveTypeReference primitiveType = EdmCoreModel.Instance.GetInt32(isNullable: true);
            Mock<ODataPrimitiveDeserializer> deserializer = new Mock<ODataPrimitiveDeserializer>(primitiveType);
            ODataProperty property = new ODataProperty();
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializer.Setup(d => d.ReadPrimitive(property, readContext)).Returns(42).Verifiable();

            // Act
            var result = deserializer.Object.ReadInline(property, readContext);

            // Assert
            deserializer.Verify();
            Assert.Equal(42, result);
        }

        [Fact]
        public void Read_ThrowsArgumentNull_MessageReader()
        {
            IEdmPrimitiveTypeReference primitiveType = EdmCoreModel.Instance.GetInt32(isNullable: true);
            var deserializer = new ODataPrimitiveDeserializer(primitiveType);

            Assert.ThrowsArgumentNull(
                () => deserializer.Read(messageReader: null, readContext: new ODataDeserializerContext()),
                "messageReader");
        }

        [Fact]
        public void ReadPrimitive_ThrowsArgumentNull_PrimitiveProperty()
        {
            IEdmPrimitiveTypeReference primitiveType = EdmCoreModel.Instance.GetInt32(isNullable: true);
            var deserializer = new ODataPrimitiveDeserializer(primitiveType);

            Assert.ThrowsArgumentNull(
                () => deserializer.ReadPrimitive(primitiveProperty: null, readContext: new ODataDeserializerContext()),
                "primitiveProperty");
        }

        [Theory]
        [TestDataSet(typeof(ODataPrimitiveSerializerTests), "EdmPrimitiveData")]
        public void Read_Primitive(object obj)
        {
            // Arrange
            IEdmPrimitiveTypeReference primitive = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(int));

            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer(primitive);
            ODataPrimitiveDeserializer deserializer = new ODataPrimitiveDeserializer(primitive);

            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);

            serializer.WriteObject(
                obj,
                new ODataMessageWriter(message as IODataResponseMessage, new ODataMessageWriterSettings(), EdmCoreModel.Instance),
                new ODataSerializerContext { RootElementName = "Property" });
            stream.Seek(0, SeekOrigin.Begin);
            Assert.Equal(
                obj,
                deserializer.Read(
                new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), EdmCoreModel.Instance),
                new ODataDeserializerContext()));
        }

        [Theory]
        [PropertyData("NonEdmPrimitiveData")]
        public void Read_MappedPrimitive(object obj, object expected)
        {
            // Arrange
            IEdmPrimitiveTypeReference primitive = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(int));

            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer(primitive);
            ODataPrimitiveDeserializer deserializer = new ODataPrimitiveDeserializer(primitive);

            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);

            serializer.WriteObject(
                obj,
                new ODataMessageWriter(message as IODataResponseMessage, new ODataMessageWriterSettings(), EdmCoreModel.Instance),
                new ODataSerializerContext { RootElementName = "Property" });
            stream.Seek(0, SeekOrigin.Begin);

            // Act && Assert
            Assert.Equal(
                expected,
                deserializer.Read(
                new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), EdmCoreModel.Instance),
                new ODataDeserializerContext()));
        }
    }
}
