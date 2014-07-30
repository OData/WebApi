// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Formatter
{
    public class ODataRawValueMediaTypeMappingTests
    {
        public static TheoryDataSet<ODataRawValueMediaTypeMapping> ODataRawValueMediaTypeMappings
        {
            get
            {
                return new TheoryDataSet<ODataRawValueMediaTypeMapping>
                {
                    new ODataPrimitiveValueMediaTypeMapping(),
                    new ODataBinaryValueMediaTypeMapping(),
                    new ODataEnumValueMediaTypeMapping()
                };
            }
        }

        [Theory]
        [PropertyData("ODataRawValueMediaTypeMappings")]
        public void TryMatchMediaType_ThrowsArgumentNull_WhenRequestIsNull(ODataRawValueMediaTypeMapping mapping)
        {
            // Arrange, Act & Assert
            Assert.ThrowsArgumentNull(() => { mapping.TryMatchMediaType(null); }, "request");
        }

        [Fact]
        public void TryMatchMediaTypeWithPrimitiveRawValueMatchesRequest()
        {
            IEdmModel model = ODataTestUtil.GetEdmModel();
            PropertyAccessPathSegment propertySegment = new PropertyAccessPathSegment((model.GetEdmType(typeof(FormatterPerson)) as IEdmEntityType).FindProperty("Age"));
            ODataPath path = new ODataPath(new EntitySetPathSegment("People"), new KeyValuePathSegment("1"), propertySegment, new ValuePathSegment());
            ODataPrimitiveValueMediaTypeMapping mapping = new ODataPrimitiveValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/People(1)/Age/$value");
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = path;

            double mapResult = mapping.TryMatchMediaType(request);

            Assert.Equal(1.0, mapResult);
        }

        [Theory]
        [PropertyData("ODataRawValueMediaTypeMappings")]
        public void TryMatchMediaType_DoesntMatchRequest_WithNonODataRequest(ODataRawValueMediaTypeMapping mapping)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");

            // Act
            double mapResult = mapping.TryMatchMediaType(request);

            // Assert
            Assert.Equal(0, mapResult);
        }

        [Fact]
        public void TryMatchMediaType_WithNonRawvalueRequest_DoesntMatchRequest()
        {
            IEdmModel model = ODataTestUtil.GetEdmModel();
            PropertyAccessPathSegment propertySegment = new PropertyAccessPathSegment((model.GetEdmType(typeof(FormatterPerson)) as IEdmEntityType).FindProperty("Age"));
            ODataPath path = new ODataPath(new EntitySetPathSegment("People"), new KeyValuePathSegment("1"), propertySegment);
            ODataPrimitiveValueMediaTypeMapping mapping = new ODataPrimitiveValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/People(1)/Age/");
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = path;

            double mapResult = mapping.TryMatchMediaType(request);

            Assert.Equal(0, mapResult);
        }

        [Fact]
        public void TryMatchMediaType_WithNonRawvalueRequest_DoesntMatchRequest_OnSingleton()
        {
            // Arrange
            IEdmModel model = ODataTestUtil.GetEdmModel();
            PropertyAccessPathSegment propertySegment = new PropertyAccessPathSegment(
                (model.GetEdmType(typeof(FormatterPerson)) as IEdmEntityType).FindProperty("Age"));
            ODataPath path = new ODataPath(new SingletonPathSegment("President"), propertySegment);
            ODataPrimitiveValueMediaTypeMapping mapping = new ODataPrimitiveValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/President/Age/");
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = path;

            // Act
            double mapResult = mapping.TryMatchMediaType(request);

            // Assert
            Assert.Equal(0, mapResult);
        }

        [Fact]
        public void TryMatchMediaType_MatchesRequest_WithEnumRawValue()
        {
            // Arrange
            IEdmModel model = GetEnumModel();
            PropertyAccessPathSegment propertySegment = new PropertyAccessPathSegment((model.GetEdmType(typeof(EnumEntity)) as IEdmEntityType).FindProperty("EnumProperty"));
            ODataPath path = new ODataPath(new EntitySetPathSegment("EnumEntity"), new KeyValuePathSegment("1"), propertySegment, new ValuePathSegment());
            ODataEnumValueMediaTypeMapping mapping = new ODataEnumValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/EnumEntity(1)/EnumProperty/$value");
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = path;

            // Act
            double mapResult = mapping.TryMatchMediaType(request);

            // Assert
            Assert.Equal(1.0, mapResult);
        }

        [Fact]
        public void TryMatchMediaType_DoesnotMatchRequest_ODataEnumValueMediaTypeMappingWithNonRawvalueRequest()
        {
            // Arrange
            IEdmModel model = GetEnumModel();
            PropertyAccessPathSegment propertySegment = new PropertyAccessPathSegment((model.GetEdmType(typeof(EnumEntity)) as IEdmEntityType).FindProperty("EnumProperty"));
            ODataPath path = new ODataPath(new EntitySetPathSegment("EnumEntity"), new KeyValuePathSegment("1"), propertySegment);
            ODataEnumValueMediaTypeMapping mapping = new ODataEnumValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/EnumEntity(1)/EnumProperty/");
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = path;

            // Act
            double mapResult = mapping.TryMatchMediaType(request);

            // Assert
            Assert.Equal(0, mapResult);
        }

        [Fact]
        public void TryMatchMediaTypeWithBinaryRawValueMatchesRequest()
        {
            IEdmModel model = GetBinaryModel();
            PropertyAccessPathSegment propertySegment = new PropertyAccessPathSegment((model.GetEdmType(typeof(RawValueEntity)) as IEdmEntityType).FindProperty("BinaryProperty"));
            ODataPath path = new ODataPath(new EntitySetPathSegment("RawValue"), new KeyValuePathSegment("1"), propertySegment, new ValuePathSegment());
            ODataBinaryValueMediaTypeMapping mapping = new ODataBinaryValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/RawValue(1)/BinaryProperty/$value");
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = path;

            double mapResult = mapping.TryMatchMediaType(request);

            Assert.Equal(1.0, mapResult);
        }

        [Fact]
        public void TryMatchMediaTypeWithBinaryRawValueMatchesRequest_OnODataSingleton()
        {
            // Arrange
            IEdmModel model = GetBinaryModel();
            PropertyAccessPathSegment propertySegment = new PropertyAccessPathSegment(
                (model.GetEdmType(typeof(RawValueEntity)) as IEdmEntityType).FindProperty("BinaryProperty"));
            ODataPath path = new ODataPath(new SingletonPathSegment("RawSingletonValue"), propertySegment, new ValuePathSegment());
            ODataBinaryValueMediaTypeMapping mapping = new ODataBinaryValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/RawSingletonValue/BinaryProperty/$value");
            request.ODataProperties().Model = model;
            request.ODataProperties().Path = path;

            // Act
            double mapResult = mapping.TryMatchMediaType(request);

            // Assert
            Assert.Equal(1.0, mapResult);
        }

        private class RawValueEntity
        {
            public int Id { get; set; }
            public byte[] BinaryProperty { get; set; }
        }

        private static IEdmModel GetBinaryModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<RawValueEntity>("RawValue");
            builder.Singleton<RawValueEntity>("RawSingletonValue");
            return builder.GetEdmModel();
        }

        private class EnumEntity
        {
            public int Id { get; set; }
            public Color EnumProperty { get; set; }
        }

        private static IEdmModel GetEnumModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<EnumEntity>("EnumEntity");
            return builder.GetEdmModel();
        }
    }
}
