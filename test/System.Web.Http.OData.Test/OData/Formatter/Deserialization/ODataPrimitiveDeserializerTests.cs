// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Linq;
using System.IO;
using System.Web.Http.OData.Formatter.Serialization;
using System.Xml.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

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
                    { (char)'1', "1"},
                    { (char?) null, null},
                    { (char[]) new char[] {'1' }, "1"},
                    { (UInt16)1, (int)1},
                    {(UInt16?)null, null},
                    {(UInt32)1, (long)1},
                    { (UInt32?)null, null},
                    { (UInt64)1, (long)1}, 
                    { (UInt64?)null, null},
                    //(Stream) new MemoryStream(new byte[] { 1 }), // TODO: Enable once we have support for streams
                    { (XElement) new XElement(XName.Get("element","namespace")), new XElement(XName.Get("element","namespace")).ToString()},
                    { (Binary) new Binary(new byte[] {1}), new byte[] {1}}
                };
            }
        }

        [Fact]
        public void Default_Ctor()
        {
            Assert.DoesNotThrow(() => new ODataPrimitiveDeserializer(EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(int))));
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
                new ODataSerializerContext { ServiceOperationName = "Property" });
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
                new ODataSerializerContext { ServiceOperationName = "Property" });
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
