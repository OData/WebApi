﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.OData.Builder;
using System.Web.OData.Builder.TestModels;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.TestCommon;
using ODataPath = System.Web.OData.Routing.ODataPath;

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
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            IEdmEntityType personType =
                model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "FormatterPerson");

            IEdmStructuralProperty ageProperty = personType.FindProperty("Age") as IEdmStructuralProperty;
            Assert.NotNull(ageProperty); // Guard
            PropertySegment propertySegment = new PropertySegment(ageProperty);

            var keys = new[] { new KeyValuePair<string, object>("PerId", 1) };
            KeySegment keySegment = new KeySegment(keys, personType, people);
            ODataPath path = new ODataPath(new EntitySetSegment(people), keySegment,
                propertySegment, new ValueSegment(ageProperty.Type.Definition));

            ODataPrimitiveValueMediaTypeMapping mapping = new ODataPrimitiveValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/People(1)/Age/$value");
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
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            IEdmEntityType personType =
                model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "FormatterPerson");

            IEdmStructuralProperty ageProperty = personType.FindProperty("Age") as IEdmStructuralProperty;
            Assert.NotNull(ageProperty); // Guard
            PropertySegment propertySegment = new PropertySegment(ageProperty);

            var keys = new[] { new KeyValuePair<string, object>("PerId", 1) };
            KeySegment keySegment = new KeySegment(keys, personType, people);

            ODataPath path = new ODataPath(new EntitySetSegment(people), keySegment, propertySegment);
            ODataPrimitiveValueMediaTypeMapping mapping = new ODataPrimitiveValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/People(1)/Age/");
            request.ODataProperties().Path = path;

            double mapResult = mapping.TryMatchMediaType(request);

            Assert.Equal(0, mapResult);
        }

        [Fact]
        public void TryMatchMediaType_WithNonRawvalueRequest_DoesntMatchRequest_OnSingleton()
        {
            // Arrange
            IEdmModel model = ODataTestUtil.GetEdmModel();
            IEdmSingleton president = model.EntityContainer.FindSingleton("President");
                model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "FormatterPerson");

            IEdmEntityType personType =
                model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "FormatterPerson");

            IEdmStructuralProperty ageProperty = personType.FindProperty("Age") as IEdmStructuralProperty;
            Assert.NotNull(ageProperty); // Guard
            PropertySegment propertySegment = new PropertySegment(ageProperty);

            ODataPath path = new ODataPath(new SingletonSegment(president), propertySegment);
            ODataPrimitiveValueMediaTypeMapping mapping = new ODataPrimitiveValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/President/Age/");
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

            IEdmEntitySet enumEntity = model.EntityContainer.FindEntitySet("EnumEntity");
            IEdmEntityType enumEntityType =
                model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "EnumEntity");

            IEdmStructuralProperty property = enumEntityType.FindProperty("EnumProperty") as IEdmStructuralProperty;
            Assert.NotNull(property); // Guard
            PropertySegment propertySegment = new PropertySegment(property);

            var keys = new[] { new KeyValuePair<string, object>("Id", 1) };
            KeySegment keySegment = new KeySegment(keys, enumEntityType, enumEntity);

            ODataPath path = new ODataPath(new EntitySetSegment(enumEntity), keySegment, propertySegment, new ValueSegment(propertySegment.EdmType));
            ODataEnumValueMediaTypeMapping mapping = new ODataEnumValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/EnumEntity(1)/EnumProperty/$value");
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
            IEdmEntitySet enumEntity = model.EntityContainer.FindEntitySet("EnumEntity");
            IEdmEntityType enumEntityType =
                model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "EnumEntity");

            IEdmStructuralProperty property = enumEntityType.FindProperty("EnumProperty") as IEdmStructuralProperty;
            Assert.NotNull(property); // Guard
            PropertySegment propertySegment = new PropertySegment(property);

            var keys = new[] { new KeyValuePair<string, object>("Id", 1) };
            KeySegment keySegment = new KeySegment(keys, enumEntityType, enumEntity);

            ODataPath path = new ODataPath(new EntitySetSegment(enumEntity), keySegment, propertySegment);
            ODataEnumValueMediaTypeMapping mapping = new ODataEnumValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/EnumEntity(1)/EnumProperty/");
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

            IEdmEntitySet rawValues = model.EntityContainer.FindEntitySet("RawValue");
            IEdmEntityType rawValueEntity =
                model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "RawValueEntity");

            IEdmStructuralProperty property = rawValueEntity.FindProperty("BinaryProperty") as IEdmStructuralProperty;
            Assert.NotNull(property); // Guard
            PropertySegment propertySegment = new PropertySegment(property);

            var keys = new[] { new KeyValuePair<string, object>("Id", 1) };
            KeySegment keySegment = new KeySegment(keys, rawValueEntity, rawValues);

            ODataPath path = new ODataPath(new EntitySetSegment(rawValues), keySegment, propertySegment, new ValueSegment(propertySegment.EdmType));
            ODataBinaryValueMediaTypeMapping mapping = new ODataBinaryValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/RawValue(1)/BinaryProperty/$value");
            request.ODataProperties().Path = path;

            double mapResult = mapping.TryMatchMediaType(request);

            Assert.Equal(1.0, mapResult);
        }

        [Fact]
        public void TryMatchMediaTypeWithBinaryRawValueMatchesRequest_OnODataSingleton()
        {
            // Arrange
            IEdmModel model = GetBinaryModel();

            IEdmSingleton rawSingletonValue = model.EntityContainer.FindSingleton("RawSingletonValue");
            IEdmEntityType rawValueEntity =
                model.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "RawValueEntity");

            IEdmStructuralProperty property = rawValueEntity.FindProperty("BinaryProperty") as IEdmStructuralProperty;
            Assert.NotNull(property); // Guard
            PropertySegment propertySegment = new PropertySegment(property);

            ODataPath path = new ODataPath(new SingletonSegment(rawSingletonValue), propertySegment, new ValueSegment(propertySegment.EdmType));
            ODataBinaryValueMediaTypeMapping mapping = new ODataBinaryValueMediaTypeMapping();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/RawSingletonValue/BinaryProperty/$value");
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
