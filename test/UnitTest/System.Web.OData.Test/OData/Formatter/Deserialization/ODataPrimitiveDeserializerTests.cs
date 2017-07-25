﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Data.Linq;
using System.IO;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Formatter.Serialization.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Formatter.Deserialization
{
    public class ODataPrimitiveDeserializerTests
    {
        private IEdmPrimitiveTypeReference _edmIntType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false);

        public static TheoryDataSet<object, object> NonEdmPrimitiveData
        {
            get
            {
                return new TheoryDataSet<object, object>
                {
                    { (char)'1', "1" },
                    { (char[]) new char[] {'1'}, "1" },
                    { (UInt16)1, (int)1 },
                    { (UInt32)1, (long)1 },
                    { (UInt64)1, (long)1 },
                    //(Stream) new MemoryStream(new byte[] { 1 }), // TODO: Enable once we have support for streams
                    { new Binary(new byte[] {1}), new byte[] {1} }
                };
            }
        }

        public static TheoryDataSet<DateTimeOffset, DateTime, TimeZoneInfo> DateTimePrimitiveData
        {
            get
            {
                DateTime dtUtc = new DateTime(2014, 10, 16, 1, 2, 3, DateTimeKind.Utc);
                DateTime dtLocal = new DateTime(2014, 10, 16, 1, 2, 3, DateTimeKind.Local);
                DateTime dtUnspecified = new DateTime(2014, 10, 16, 1, 2, 3, DateTimeKind.Unspecified);
                TimeZoneInfo pacificStandard = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                TimeZoneInfo chinaStandard = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                return new TheoryDataSet<DateTimeOffset, DateTime, TimeZoneInfo>
                {
                    { DateTimeOffset.Parse("2014-10-16T01:02:03Z"), dtUtc, null },
                    { new DateTimeOffset(dtLocal), dtLocal, null },
                    { new DateTimeOffset(new DateTime(2014, 10, 16, 1, 2, 3, DateTimeKind.Unspecified)), dtUnspecified, null },
                    { DateTimeOffset.Parse("2014-10-16T09:02:03+8:00"), dtUtc, chinaStandard },
                    { new DateTimeOffset(dtLocal).ToOffset(new TimeSpan(+8,0,0)), dtLocal, chinaStandard },
                    { DateTimeOffset.Parse("2014-10-16T01:02:03-7:00"), dtUnspecified, pacificStandard },
                };
            }
        }

        [Fact]
        public void ReadInline_ReturnsNull_IfItemIsNull()
        {
            IEdmPrimitiveTypeReference primitiveType = EdmCoreModel.Instance.GetInt32(isNullable: true);
            var deserializer = new ODataPrimitiveDeserializer();

            Assert.Null(deserializer.ReadInline(item: null, edmType: _edmIntType, readContext: new ODataDeserializerContext()));
        }

        [Fact]
        public void ReadInline_Throws_ArgumentMustBeOfType()
        {
            IEdmPrimitiveTypeReference primitiveType = EdmCoreModel.Instance.GetInt32(isNullable: true);
            var deserializer = new ODataPrimitiveDeserializer();

            Assert.ThrowsArgument(
                () => deserializer.ReadInline(42, _edmIntType, new ODataDeserializerContext()),
                "item",
                "The argument must be of type 'ODataProperty'");
        }

        [Fact]
        public void ReadInline_Calls_ReadPrimitive()
        {
            // Arrange
            IEdmPrimitiveTypeReference primitiveType = EdmCoreModel.Instance.GetInt32(isNullable: true);
            Mock<ODataPrimitiveDeserializer> deserializer = new Mock<ODataPrimitiveDeserializer>();
            ODataProperty property = new ODataProperty();
            ODataDeserializerContext readContext = new ODataDeserializerContext();

            deserializer.Setup(d => d.ReadPrimitive(property, readContext)).Returns(42).Verifiable();

            // Act
            var result = deserializer.Object.ReadInline(property, primitiveType, readContext);

            // Assert
            deserializer.Verify();
            Assert.Equal(42, result);
        }

        [Fact]
        public void Read_ThrowsArgumentNull_MessageReader()
        {
            var deserializer = new ODataPrimitiveDeserializer();

            Assert.ThrowsArgumentNull(
                () => deserializer.Read(messageReader: null, type: typeof(int), readContext: new ODataDeserializerContext()),
                "messageReader");
        }

        [Fact]
        public void ReadPrimitive_ThrowsArgumentNull_PrimitiveProperty()
        {
            var deserializer = new ODataPrimitiveDeserializer();

            Assert.ThrowsArgumentNull(
                () => deserializer.ReadPrimitive(primitiveProperty: null, readContext: new ODataDeserializerContext()),
                "primitiveProperty");
        }

        [Theory]
        [TestDataSet(typeof(ODataPrimitiveSerializerTests), "EdmPrimitiveData")]
        public void Read_Primitive(object obj, string edmType, string value)
        {
            // Arrange
            IEdmModel model = CreateModel();
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();
            ODataPrimitiveDeserializer deserializer = new ODataPrimitiveDeserializer();

            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);

            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/"), }
            };
            settings.SetContentType(ODataFormat.Json);

            ODataMessageWriter messageWriter = new ODataMessageWriter(message as IODataResponseMessage, settings, model);
            ODataMessageReader messageReader = new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), model);
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "Property", Model = model };
            ODataDeserializerContext readContext = new ODataDeserializerContext { Model = model };

            Type type = obj == null ? typeof(int) : obj.GetType();
            serializer.WriteObject(obj, type, messageWriter, writeContext);
            stream.Seek(0, SeekOrigin.Begin);

            // Act & Assert
            Assert.Equal(obj, deserializer.Read(messageReader, type, readContext));
        }

        [Theory]
        [PropertyData("NonEdmPrimitiveData")]
        public void Read_MappedPrimitive(object obj, object expected)
        {
            // Arrange
            IEdmModel model = CreateModel();
            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();
            ODataPrimitiveDeserializer deserializer = new ODataPrimitiveDeserializer();

            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);

            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/"), }
            };
            settings.SetContentType(ODataFormat.Json);

            ODataMessageWriter messageWriter = new ODataMessageWriter(message as IODataResponseMessage, settings, model);
            ODataMessageReader messageReader = new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), model);
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "Property", Model = model };
            ODataDeserializerContext readContext = new ODataDeserializerContext { Model = model };

            Type type = obj == null ? typeof(int) : expected.GetType();
            serializer.WriteObject(obj, type, messageWriter, writeContext);
            stream.Seek(0, SeekOrigin.Begin);

            // Act && Assert
            Assert.Equal(expected, deserializer.Read(messageReader, type, readContext));
        }

        [Theory]
        [PropertyData("DateTimePrimitiveData")]
        public void Read_DateTimePrimitive(DateTimeOffset expected, DateTime value, TimeZoneInfo timeZoneInfo)
        {
            // Arrange
            IEdmModel model = CreateModel();

            HttpRequestMessage request = new HttpRequestMessage();
            request.SetConfiguration(new HttpConfiguration());
            if (timeZoneInfo != null)
            {
                request.GetConfiguration().SetTimeZoneInfo(timeZoneInfo);
            }
            else
            {
                request.GetConfiguration().SetTimeZoneInfo(TimeZoneInfo.Local);
            }

            ODataPrimitiveSerializer serializer = new ODataPrimitiveSerializer();
            ODataPrimitiveDeserializer deserializer = new ODataPrimitiveDeserializer();

            MemoryStream stream = new MemoryStream();
            ODataMessageWrapper message = new ODataMessageWrapper(stream);

            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/"), }
            };
            settings.SetContentType(ODataFormat.Json);

            ODataMessageWriter messageWriter = new ODataMessageWriter(message as IODataResponseMessage, settings, model);
            ODataMessageReader messageReader = new ODataMessageReader(message as IODataResponseMessage, new ODataMessageReaderSettings(), model);
            ODataSerializerContext writeContext = new ODataSerializerContext { RootElementName = "Property", Model = model, Request = request };
            ODataDeserializerContext readContext = new ODataDeserializerContext { Model = model, Request = request };

            serializer.WriteObject(value, typeof(DateTimeOffset), messageWriter, writeContext);
            stream.Seek(0, SeekOrigin.Begin);

            // Act && Assert
            Assert.Equal(expected, deserializer.Read(messageReader, typeof(DateTimeOffset), readContext));
        }

        private static IEdmModel CreateModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            return builder.GetEdmModel();
        }
    }
}
