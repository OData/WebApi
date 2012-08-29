// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Xml.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataPrimitiveSerializerTests
    {
        public static IEnumerable<object[]> NonEdmPrimitiveConversionData
        {
            get
            {
                return ODataEntryDeserializerTests
                    .ConvertPrimitiveValue_NonStandardPrimitives_Data
                    .Select(data => new[] { data[1], data[0] });
            }
        }

        public static TheoryDataSet<object> NonEdmPrimitiveData
        {
            get
            {
                return new TheoryDataSet<object>
                {
                    (char)'1',
                    (char?) null,
                    (char[]) new char[] {'1' },
                    (UInt16)1,
                    (UInt16?)null,
                    (UInt32)1,
                    (UInt32?)null,
                    (UInt64)1,
                    (UInt64?)null,
                    //(Stream) new MemoryStream(new byte[] { 1 }), // TODO: Enable once we have support for streams
                    (XElement) new XElement(XName.Get("element","namespace")), 
                    (Binary) new Binary(new byte[] {1})
                };
            }
        }

        public static TheoryDataSet<object> EdmPrimitiveData
        {
            get
            {
                return new TheoryDataSet<object>
                {
                    (string)"1",
                    (Boolean)true,
                    (Boolean?)null,
                    (Byte)1,
                    (Byte?)null,
                    (DateTime)DateTime.Now,
                    (DateTime?)null,
                    (Decimal)1,
                    (Decimal?)null,
                    (Double)1,
                    (Double?)null,
                    (Guid)Guid.Empty,
                    (Guid?)null,
                    (Int16)1,
                    (Int16?)null,
                    (Int32)1,
                    (Int32?)null,
                    (Int64)1,
                    (Int64?)null,
                    (SByte)1,
                    (SByte?)null,
                    (Single)1,
                    (Single?)null,
                    (byte[])new byte[] { 1 },
                     (TimeSpan) new TimeSpan(),
                    (TimeSpan?) null,
                    (DateTimeOffset) new DateTimeOffset(),
                    (DateTimeOffset?)null
                };
            }
        }

        [Fact]
        public void Constructor_ThrowsArgumentNull_edmPrimitiveType()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                var serializer = new ODataPrimitiveSerializer(edmPrimitiveType: null);
            }, "edmType");
        }

        [Theory]
        [PropertyData("EdmPrimitiveKinds")]
        public void Constructor_SucceedsForValidPrimitiveType(EdmPrimitiveTypeKind primitiveTypeKind)
        {
            IEdmPrimitiveType edmPrimitiveType = EdmCoreModel.Instance.SchemaElements
                                                                .OfType<IEdmPrimitiveType>()
                                                                .Where(primitiveType => primitiveType.PrimitiveKind == primitiveTypeKind)
                                                                .FirstOrDefault();
            IEdmPrimitiveTypeReference edmPrimitiveTypeReference = new EdmPrimitiveTypeReference(edmPrimitiveType, false);

            var serializer = new ODataPrimitiveSerializer(edmPrimitiveTypeReference);

            Assert.Equal(serializer.EdmType, edmPrimitiveTypeReference);
            Assert.Equal(serializer.ODataPayloadKind, ODataPayloadKind.Property);
        }

        [Fact]
        public void CreateProperty()
        {
            IEdmPrimitiveTypeReference edmPrimitiveType = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(int));
            var serializer = new ODataPrimitiveSerializer(edmPrimitiveType);

            var odataProperty = serializer.CreateProperty(20, "elementName", writeContext: null);
            Assert.NotNull(odataProperty);
            Assert.Equal(odataProperty.Name, "elementName");
            Assert.Equal(odataProperty.Value, 20);
        }

        [Theory]
        [PropertyData("EdmPrimitiveData")]
        [PropertyData("NonEdmPrimitiveData")]
        public void WriteObject_EdmPrimitives(object graph)
        {
            IEdmPrimitiveTypeReference edmPrimitiveType = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(int));
            var serializer = new ODataPrimitiveSerializer(edmPrimitiveType);

            ODataMessageWriter writer = new ODataMessageWriter(new ODataMessageWrapper(new MemoryStream()) as IODataResponseMessage);

            Assert.DoesNotThrow(() => serializer.WriteObject(graph, writer, new ODataSerializerContext() { ServiceOperationName = "PropertyName" }));
        }

        [Theory]
        [PropertyData("EdmPrimitiveData")]
        public void ConvertUnsupportedPrimitives_DoesntChangeStandardEdmPrimitives(object graph)
        {
            Assert.Equal(
                graph,
                ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(graph));
        }

        [Theory]
        [PropertyData("NonEdmPrimitiveConversionData")]
        public void ConvertUnsupportedPrimitives_NonStandardEdmPrimitives(object graph, object result)
        {
            Assert.Equal(
                result,
                ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(graph));
        }

        public static TheoryDataSet<EdmPrimitiveTypeKind> EdmPrimitiveKinds
        {
            get
            {
                TheoryDataSet<EdmPrimitiveTypeKind> dataset = new TheoryDataSet<EdmPrimitiveTypeKind>();
                var primitiveKinds = Enum.GetValues(typeof(EdmPrimitiveTypeKind))
                                        .OfType<EdmPrimitiveTypeKind>()
                                        .Where(primitiveKind => primitiveKind != EdmPrimitiveTypeKind.None);

                foreach (var primitiveKind in primitiveKinds)
                {
                    dataset.Add(primitiveKind);
                }
                return dataset;
            }
        }
    }
}
