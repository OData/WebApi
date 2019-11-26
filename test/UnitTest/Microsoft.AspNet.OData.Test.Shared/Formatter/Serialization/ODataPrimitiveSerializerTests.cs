// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
#if NETFX // Binary only supported on Net Framework
using System.Data.Linq;
#endif
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    public class ODataPrimitiveSerializerTests
    {
        public static IEnumerable<object[]> NonEdmPrimitiveConversionData
        {
            get
            {
                return EdmPrimitiveHelpersTest
                    .ConvertPrimitiveValue_NonStandardPrimitives_Data
                    .Select(data => new[] { data[1], data[0] });
            }
        }

        public static TheoryDataSet<DateTime> NonEdmPrimitiveConversionDateTime
        {
            get
            {
                DateTime dtUtc = new DateTime(2014, 12, 12, 1, 2, 3, DateTimeKind.Utc);
                DateTime dtLocal = new DateTime(2014, 12, 12, 1, 2, 3, DateTimeKind.Local);
                DateTime unspecified = new DateTime(2014, 12, 12, 1, 2, 3, DateTimeKind.Unspecified);
                return new TheoryDataSet<DateTime>
                {
                    { dtUtc },
                    { dtLocal },
                    { unspecified },
                };
            }
        }

        public static TheoryDataSet<object, string, string> NonEdmPrimitiveData
        {
            get
            {
                return new TheoryDataSet<object, string, string>
                {
                    { (char)'1', "Edm.String", "\"1\"" },
                    { (char[]) new char[] {'1' }, "Edm.String", "\"1\"" },
                    { (UInt16)1, "Edm.Int32", "1" },
                    { (UInt32)1, "Edm.Int64", "1" },
                    { (UInt64)1, "Edm.Int64", "1" },
                    //(Stream) new MemoryStream(new byte[] { 1 }), // TODO: Enable once we have support for streams
                    { new XElement(XName.Get("element","namespace")), "Edm.String", "\"<element xmlns=\\\"namespace\\\" />\"" },
#if NETFX // Binary only supported on Net Framework
                    { new Binary(new byte[] {1}), "Edm.Binary", "\"AQ==\"" },
#endif
                };
            }
        }

        public static TheoryDataSet<object, string, string> EdmPrimitiveData
        {
            get
            {
                return new TheoryDataSet<object, string, string>
                {
                    { "1", "Edm.String", "\"1\"" },
                    { true, "Edm.Boolean", "true" },
                    { (Byte)1, "Edm.Byte", "1" },
                    { (Decimal)1, "Edm.Decimal", "1" },
                    { (Double)1, "Edm.Double", "1.0" },
                    { (Guid)Guid.Empty, "Edm.Guid", "\"00000000-0000-0000-0000-000000000000\"" },
                    { (Int16)1, "Edm.Int16", "1" },
                    { (Int32)1, "Edm.Int32", "1" },
                    { (Int64)1, "Edm.Int64", "1" },
                    { (SByte)1, "Edm.SByte", "1" },
                    { (Single)1, "Edm.Single", "1" },
                    { new byte[] { 1 }, "Edm.Binary", "\"AQ==\"" },
                    { new TimeSpan(), "Edm.Duration", "\"PT0S\"" },
                    { new DateTimeOffset(), "Edm.DateTimeOffset", "\"0001-01-01T00:00:00Z\"" },
                    { new Date(2014, 10, 13), "Edm.Date", "\"2014-10-13\"" },
                    { new TimeOfDay(15, 38, 25, 109), "Edm.TimeOfDay", "\"15:38:25.1090000\"" },
                };
            }
        }

        [Fact]
        public void Property_ODataPayloadKind()
        {
            var serializer = new ODataPrimitiveSerializer();
            Assert.Equal(ODataPayloadKind.Property, serializer.ODataPayloadKind);
        }

        [Fact]
        public void WriteObject_Throws_RootElementNameMissing()
        {
            ODataSerializerContext writeContext = new ODataSerializerContext();
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();

#if NETCOREAPP3_0
            ExceptionAssert.Throws<ArgumentException>(
                () => serializer.WriteObject(42, typeof(int), ODataTestUtil.GetMockODataMessageWriter(), writeContext),
                "The 'RootElementName' property is required on 'ODataSerializerContext'. (Parameter 'writeContext')");
#else
            ExceptionAssert.Throws<ArgumentException>(
                () => serializer.WriteObject(42, typeof(int), ODataTestUtil.GetMockODataMessageWriter(), writeContext),
                "The 'RootElementName' property is required on 'ODataSerializerContext'.\r\nParameter name: writeContext");
#endif
        }

        [Fact]
        public void WriteObject_Calls_CreateODataPrimitiveValue()
        {
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "Property", Model = EdmCoreModel.Instance };
            Mock<ODataPrimitiveSerializer> serializer = new Mock<ODataPrimitiveSerializer>();
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataPrimitiveValue(
                    42, It.Is<IEdmPrimitiveTypeReference>(t => t.PrimitiveKind() == EdmPrimitiveTypeKind.Int32), writeContext))
                .Returns(new ODataPrimitiveValue(42)).Verifiable();

            serializer.Object.WriteObject(42, typeof(int), ODataTestUtil.GetMockODataMessageWriter(), writeContext);

            serializer.Verify();
        }

        [Fact]
        public void CreateODataValue_PrimitiveValue()
        {
            IEdmPrimitiveTypeReference edmPrimitiveType = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(int));
            var serializer = new ODataPrimitiveSerializer();

            var odataValue = serializer.CreateODataValue(20, edmPrimitiveType, writeContext: null);
            Assert.NotNull(odataValue);
            ODataPrimitiveValue primitiveValue = Assert.IsType<ODataPrimitiveValue>(odataValue);
            Assert.Equal(20, primitiveValue.Value);
        }

        [Fact]
        public void CreateODataValue_ReturnsODataNullValue_ForNullValue()
        {
            IEdmPrimitiveTypeReference edmPrimitiveType = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(string));
            var serializer = new ODataPrimitiveSerializer();
            var odataValue = serializer.CreateODataValue(null, edmPrimitiveType, new ODataSerializerContext());

            Assert.IsType<ODataNullValue>(odataValue);
        }

        [Fact]
        public void CreateODataValue_ReturnsDateTimeOffset_ForDateTime_ByDefault()
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType =
                EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(DateTime));
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();
            DateTime dt = new DateTime(2014, 10, 27);
            TimeZoneInfoHelper.TimeZone = null;

            // Act
            ODataValue odataValue = serializer.CreateODataValue(dt, edmPrimitiveType, new ODataSerializerContext());

            // Assert
            ODataPrimitiveValue primitiveValue = Assert.IsType<ODataPrimitiveValue>(odataValue);
            Assert.Equal(new DateTimeOffset(dt), primitiveValue.Value);
        }

        [Theory]
        [InlineData("UTC")] // +0:00
        [InlineData("Pacific Standard Time")] // -8:00
        [InlineData("China Standard Time")] // +8:00
        public void CreateODataValue_ReturnsDateTimeOffsetMinValue_ForDateTimeMinValue(string timeZoneId)
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType =
                EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(DateTime));
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();
            DateTime dt = DateTime.MinValue;
            TimeZoneInfoHelper.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            // Act
            ODataValue odataValue = serializer.CreateODataValue(dt, edmPrimitiveType, new ODataSerializerContext());

            // Assert
            ODataPrimitiveValue primitiveValue = Assert.IsType<ODataPrimitiveValue>(odataValue);

            if (TimeZoneInfoHelper.TimeZone.BaseUtcOffset.Hours < 0)
            {
                Assert.Equal(new DateTimeOffset(dt, TimeZoneInfoHelper.TimeZone.GetUtcOffset(dt)), primitiveValue.Value);
            }
            else
            {
                Assert.Equal(DateTimeOffset.MinValue, primitiveValue.Value);
            }
        }

        [Theory]
        [InlineData("UTC")] // +0:00
        [InlineData("Pacific Standard Time")] // -8:00
        [InlineData("China Standard Time")] // +8:00
        public void CreateODataValue_ReturnsDateTimeOffsetMaxValue_ForDateTimeMaxValue(string timeZoneId)
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType =
                EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(DateTime));
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();
            DateTime dt = DateTime.MaxValue;
            TimeZoneInfoHelper.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            // Act
            ODataValue odataValue = serializer.CreateODataValue(dt, edmPrimitiveType, new ODataSerializerContext());

            // Assert
            ODataPrimitiveValue primitiveValue = Assert.IsType<ODataPrimitiveValue>(odataValue);

            if (TimeZoneInfoHelper.TimeZone.BaseUtcOffset.Hours > 0)
            {
                Assert.Equal(new DateTimeOffset(dt, TimeZoneInfoHelper.TimeZone.GetUtcOffset(dt)), primitiveValue.Value);
            }
            else
            {
                Assert.Equal(DateTimeOffset.MaxValue, primitiveValue.Value);
            }
        }

        [Theory]
        [MemberData(nameof(NonEdmPrimitiveConversionDateTime))]
        public void CreateODataValue_ReturnsDateTimeOffset_ForDateTime_WithDifferentTimeZone(DateTime value)
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType =
                EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(DateTime));
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            configuration.SetTimeZoneInfo(tzi);

            var request = RequestFactory.Create(configuration, "OData");

            ODataSerializerContext context = new ODataSerializerContext{ Request = request };

            DateTimeOffset expected = value.Kind == DateTimeKind.Unspecified
                ? new DateTimeOffset(value, TimeZoneInfoHelper.TimeZone.GetUtcOffset(value))
                : TimeZoneInfo.ConvertTime(new DateTimeOffset(value), TimeZoneInfoHelper.TimeZone);

            // Act
            ODataValue odataValue = serializer.CreateODataValue(value, edmPrimitiveType, context);

            // Assert
            ODataPrimitiveValue primitiveValue = Assert.IsType<ODataPrimitiveValue>(odataValue);
            Assert.Equal(expected, primitiveValue.Value);
        }

        [Fact]
        public void CreateODataValue_ReturnsDate_ForDateTime()
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(Date));
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();
            DateTime dt = new DateTime(2014, 10, 27);

            // Act
            ODataValue odataValue = serializer.CreateODataValue(dt, edmPrimitiveType, new ODataSerializerContext());

            // Assert
            ODataPrimitiveValue primitiveValue = Assert.IsType<ODataPrimitiveValue>(odataValue);
            Assert.IsType<Date>(primitiveValue.Value);
            Assert.Equal(new Date(dt.Year, dt.Month, dt.Day), primitiveValue.Value);
        }

        [Fact]
        public void CreateODataValue_ReturnsTimeOfDay_ForTimeSpan()
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(TimeOfDay));
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();
            TimeSpan ts = new TimeSpan(0, 10, 11, 12, 13);

            // Act
            ODataValue odataValue = serializer.CreateODataValue(ts, edmPrimitiveType, new ODataSerializerContext());

            // Assert
            ODataPrimitiveValue primitiveValue = Assert.IsType<ODataPrimitiveValue>(odataValue);
            Assert.IsType<TimeOfDay>(primitiveValue.Value);
            Assert.Equal(new TimeOfDay(ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds), primitiveValue.Value);
        }

        [Theory]
        [MemberData(nameof(EdmPrimitiveData))]
        [MemberData(nameof(NonEdmPrimitiveData))]
        public void WriteObject_EdmPrimitives(object graph, string type, string value)
        {
            // Arrange
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();
            ODataSerializerContext writecontext = new ODataSerializerContext()
            {
                RootElementName = "PropertyName",
                Model = EdmCoreModel.Instance
            };

            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/"), }
            };

            MemoryStream stream = new MemoryStream();
            ODataMessageWriter writer = new ODataMessageWriter(
                new ODataMessageWrapper(stream) as IODataResponseMessage, settings);

            string expect = "{\"@odata.context\":\"http://any/$metadata#" + type + "\",";
            if (type == "Edm.Null")
            {
                expect += "\"@odata.null\":" + value + "}";
            }
            else
            {
                expect += "\"value\":" + value + "}";
            }

            // Act
            serializer.WriteObject(graph, typeof(int), writer, writecontext);

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            string result = new StreamReader(stream).ReadToEnd();
            Assert.Equal(expect, result);
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsAnnotation_InJsonLightMetadataMode()
        {
            // Arrange
            IEdmPrimitiveTypeReference edmPrimitiveType = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(short));
            ODataPrimitiveValue primitive = new ODataPrimitiveValue((short)1);

            // Act
            ODataPrimitiveSerializer.AddTypeNameAnnotationAsNeeded(primitive, edmPrimitiveType, ODataMetadataLevel.FullMetadata);

            // Assert
            ODataTypeAnnotation annotation = primitive.TypeAnnotation;
            Assert.NotNull(annotation); // Guard
            Assert.Equal("Edm.Int16", annotation.TypeName);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(0, true)]
        [InlineData("", true)]
        [InlineData(0.1D, true)]
        [InlineData(double.PositiveInfinity, false)]
        [InlineData(double.NegativeInfinity, false)]
        [InlineData(double.NaN, false)]
        [InlineData((short)1, false)]
        public void CanTypeBeInferredInJson(object value, bool expectedResult)
        {
            // Act
            bool actualResult = ODataPrimitiveSerializer.CanTypeBeInferredInJson(value);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void CreatePrimitive_ReturnsNull_ForNullValue()
        {
            // Act
            IEdmPrimitiveTypeReference edmPrimitiveType = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(typeof(int));
            ODataValue value = ODataPrimitiveSerializer.CreatePrimitive(null, edmPrimitiveType, writeContext: null);

            // Assert
            Assert.Null(value);
        }

        [Theory]
        [MemberData(nameof(EdmPrimitiveData))]
        public void ConvertUnsupportedPrimitives_DoesntChangeStandardEdmPrimitives(object graph, string type, string value)
        {
            Assert.NotNull(type);
            Assert.NotNull(value);
            Assert.Equal(
                graph,
                ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(graph));
        }

        [Theory]
        [MemberData(nameof(NonEdmPrimitiveConversionData))]
        public void ConvertUnsupportedPrimitives_NonStandardEdmPrimitives(object graph, object result)
        {
            Assert.Equal(
                result,
                ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(graph));
        }

        [Theory]
        [MemberData(nameof(NonEdmPrimitiveConversionDateTime))]
        public void ConvertUnsupportedDateTime_NonStandardEdmPrimitives(DateTime graph)
        {
            // Arrange & Act
            TimeZoneInfoHelper.TimeZone = null;
            object value = ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(graph);

            DateTimeOffset expected = graph.Kind == DateTimeKind.Unspecified
                ? new DateTimeOffset(graph, TimeZoneInfoHelper.TimeZone.GetUtcOffset(graph))
                : TimeZoneInfo.ConvertTime(new DateTimeOffset(graph), TimeZoneInfoHelper.TimeZone);

            // Assert
            DateTimeOffset actual = Assert.IsType<DateTimeOffset>(value);
            Assert.Equal(new DateTimeOffset(graph), actual);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(NonEdmPrimitiveConversionDateTime))]
        public void ConvertUnsupportedDateTime_NonStandardEdmPrimitives_TimeZone(DateTime graph)
        {
            // Arrange
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            TimeZoneInfoHelper.TimeZone = tzi;

            DateTimeOffset expected = graph.Kind == DateTimeKind.Unspecified
                ? new DateTimeOffset(graph, TimeZoneInfoHelper.TimeZone.GetUtcOffset(graph))
                : TimeZoneInfo.ConvertTime(new DateTimeOffset(graph), TimeZoneInfoHelper.TimeZone);

            // Act
            object value = ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(graph);

            // Assert
            DateTimeOffset actual = Assert.IsType<DateTimeOffset>(value);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(0, TestODataMetadataLevel.FullMetadata, true)]
        [InlineData((short)1, TestODataMetadataLevel.FullMetadata, false)]
        [InlineData((short)1, TestODataMetadataLevel.MinimalMetadata, true)]
        [InlineData((short)1, TestODataMetadataLevel.NoMetadata, true)]
        public void ShouldSuppressTypeNameSerialization(object value, TestODataMetadataLevel metadataLevel,
            bool expectedResult)
        {
            // Act
            bool actualResult = ODataPrimitiveSerializer.ShouldSuppressTypeNameSerialization(value,
                (ODataMetadataLevel)metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
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
