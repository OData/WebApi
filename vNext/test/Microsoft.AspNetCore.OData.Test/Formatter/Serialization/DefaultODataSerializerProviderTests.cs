// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Models;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Spatial;
using Xunit;
using ODataPath = Microsoft.AspNetCore.OData.Routing.ODataPath;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    public class DefaultODataSerializerProviderTests
    {
        [Fact]
        public void GetEdmTypeSerializer_ThrowsArgumentNull_Context()
        {
            // Arrange
            DefaultODataSerializerProvider serialierProvider = new DefaultODataSerializerProvider();

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => serialierProvider.GetEdmTypeSerializer(context: null, edmType: null));
        }

        [Fact]
        public void GetEdmTypeSerializer_ThrowsArgumentNull_EdmType()
        {
            // Arrange
            HttpContext context = new DefaultHttpContext();
            DefaultODataSerializerProvider serialierProvider = new DefaultODataSerializerProvider();

            // Act & Assert
            Assert.Throws<ArgumentNullException>("edmType", () => serialierProvider.GetEdmTypeSerializer(context, edmType: null));
        }

        [Fact]
        public void GetODataPayloadSerializer_ThrowsArgumentNull_Context()
        {
            // Arrange
            DefaultODataSerializerProvider serialierProvider = new DefaultODataSerializerProvider();

            // Act & Assert
            Assert.Throws<ArgumentNullException>("context", () => serialierProvider.GetODataPayloadSerializer(context: null, type: typeof(int)));
        }

        [Fact]
        public void GetODataPayloadSerializer_ThrowsArgumentNull_Type()
        {
            // Arrange
            HttpContext context = new DefaultHttpContext();
            DefaultODataSerializerProvider serialierProvider = new DefaultODataSerializerProvider();

            // Act & Assert
            Assert.Throws<ArgumentNullException>("type", () => serialierProvider.GetODataPayloadSerializer(context, type: null));
        }

        public static IEnumerable<object[]> EdmPrimitiveMappingData
        {
            get
            {
                yield return new object[] { typeof(byte[]), EdmPrimitiveTypeKind.Binary };
                yield return new object[] { typeof(byte), EdmPrimitiveTypeKind.Byte };
                yield return new object[] { typeof(bool), EdmPrimitiveTypeKind.Boolean };
                yield return new object[] { typeof(Guid), EdmPrimitiveTypeKind.Guid };
                yield return new object[] { typeof(float), EdmPrimitiveTypeKind.Single };
                yield return new object[] { typeof(double), EdmPrimitiveTypeKind.Double };
                yield return new object[] { typeof(short), EdmPrimitiveTypeKind.Int16 };
                yield return new object[] { typeof(int), EdmPrimitiveTypeKind.Int32 };
                yield return new object[] { typeof(long), EdmPrimitiveTypeKind.Int64 };
                yield return new object[] { typeof(sbyte), EdmPrimitiveTypeKind.SByte };
                yield return new object[] { typeof(decimal), EdmPrimitiveTypeKind.Decimal };
                yield return new object[] { typeof(DateTime), EdmPrimitiveTypeKind.DateTimeOffset };
                yield return new object[] { typeof(DateTimeOffset), EdmPrimitiveTypeKind.DateTimeOffset };
                yield return new object[] { typeof(Date), EdmPrimitiveTypeKind.Date };
                yield return new object[] {typeof (TimeOfDay), EdmPrimitiveTypeKind.TimeOfDay};
                yield return new object[] { typeof(Stream), EdmPrimitiveTypeKind.Stream };
                yield return new object[] { typeof(string), EdmPrimitiveTypeKind.String };
                yield return new object[] { typeof(TimeSpan), EdmPrimitiveTypeKind.Duration };
                yield return new object[] { typeof(GeographyPoint), EdmPrimitiveTypeKind.GeographyPoint };
            }
        }

        [Theory, MemberData("EdmPrimitiveMappingData")]
        public static void GetODataSerializer_Primitive(Type type, EdmPrimitiveTypeKind edmPrimitiveTypeKind)
        {
            // Arrange
            DefaultODataSerializerProvider serialierProvider = new DefaultODataSerializerProvider();
            HttpContext context = CreateHttpContext(serialierProvider);

            // Act
            var serializer = serialierProvider.GetODataPayloadSerializer(context, type);

            // Assert
            Assert.NotNull(serializer);
            var primitiveSerializer = Assert.IsType<ODataPrimitiveSerializer>(serializer);
            Assert.Equal(primitiveSerializer.ODataPayloadKind, ODataPayloadKind.Property);
        }

        [Theory, MemberData("EdmPrimitiveMappingData")]
        public void GetODataPayloadSerializer_ReturnsRawValueSerializer_ForValueRequests(Type type, EdmPrimitiveTypeKind edmPrimitiveTypeKind)
        {
            // Arrange
            DefaultODataSerializerProvider serialierProvider = new DefaultODataSerializerProvider();
            HttpContext context = CreateHttpContext(serialierProvider);
            context.ODataFeature().Path =
                new ODataPath(new ValueSegment(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32)));

            // Act
            var serializer = serialierProvider.GetODataPayloadSerializer(context, type);

            // Assert
            Assert.NotNull(serializer);
            Assert.Equal(ODataPayloadKind.Value, serializer.ODataPayloadKind);
        }

        [Fact]
        public void GetODataSerializer_Enum()
        {
            // Arrange
            DefaultODataSerializerProvider serialierProvider = new DefaultODataSerializerProvider();
            HttpContext context = CreateHttpContext(serialierProvider);
            context.ODataFeature().Model = GetEdmModel();

            // Act
            var serializer = serialierProvider.GetODataPayloadSerializer(context, typeof(TestEnum));

            // Assert
            Assert.NotNull(serializer);
            var enumSerializer = Assert.IsType<ODataEnumSerializer>(serializer);
            Assert.Equal(ODataPayloadKind.Property, enumSerializer.ODataPayloadKind);
        }

        [Fact]
        public void GetODataPayloadSerializer_ReturnsRawValueSerializer_ForEnumValueRequests()
        {
            // Arrange
            DefaultODataSerializerProvider serialierProvider = new DefaultODataSerializerProvider();
            HttpContext context = CreateHttpContext(serialierProvider);
            context.ODataFeature().Path =
                new ODataPath(new ValueSegment(EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32)));
            context.ODataFeature().Model = GetEdmModel();

            // Act
            var serializer = serialierProvider.GetODataPayloadSerializer(context, typeof(TestEnum));

            // Assert
            Assert.NotNull(serializer);
            var rawValueSerializer = Assert.IsType<ODataRawValueSerializer>(serializer);
            Assert.Equal(ODataPayloadKind.Value, rawValueSerializer.ODataPayloadKind);
        }

        [Theory]
        [InlineData("DollarCountEntities/$count", typeof(ODataCountTest.DollarCountEntity))]
        [InlineData("DollarCountEntities(5)/StringCollectionProp/$count", typeof(string))]
        [InlineData("DollarCountEntities(5)/EnumCollectionProp/$count", typeof(Color))]
        [InlineData("DollarCountEntities(5)/TimeSpanCollectionProp/$count", typeof(TimeSpan))]
        [InlineData("DollarCountEntities(5)/ComplexCollectionProp/$count", typeof(ODataCountTest.DollarCountComplex))]
        [InlineData("DollarCountEntities(5)/EntityCollectionProp/$count", typeof(ODataCountTest.DollarCountEntity))]
        [InlineData("UnboundFunctionReturnsPrimitveCollection()/$count", typeof(int))]
        [InlineData("UnboundFunctionReturnsEnumCollection()/$count", typeof(Color))]
        [InlineData("UnboundFunctionReturnsDateTimeOffsetCollection()/$count", typeof(DateTimeOffset))]
        [InlineData("UnboundFunctionReturnsDateCollection()/$count", typeof(Date))]
        [InlineData("UnboundFunctionReturnsComplexCollection()/$count", typeof(ODataCountTest.DollarCountComplex))]
        [InlineData("UnboundFunctionReturnsEntityCollection()/$count", typeof(ODataCountTest.DollarCountEntity))]
        [InlineData("DollarCountEntities/Default.BoundFunctionReturnsPrimitveCollection()/$count", typeof(DateTimeOffset))]
        [InlineData("DollarCountEntities/Default.BoundFunctionReturnsEnumCollection()/$count", typeof(Color))]
        [InlineData("DollarCountEntities/Default.BoundFunctionReturnsDateTimeOffsetCollection()/$count", typeof(DateTimeOffset))]
        [InlineData("DollarCountEntities/Default.BoundFunctionReturnsComplexCollection()/$count", typeof(ODataCountTest.DollarCountComplex))]
        [InlineData("DollarCountEntities/Default.BoundFunctionReturnsEntityCollection()/$count", typeof(ODataCountTest.DollarCountEntity))]
        public void GetODataPayloadSerializer_ReturnsRawValueSerializer_ForDollarCountRequests(string uri, Type elementType)
        {
            // Arrange
            IEdmModel model = ODataCountTest.GetEdmModel();
            Type type = typeof(ICollection<>).MakeGenericType(elementType);
            DefaultODataSerializerProvider serialierProvider = new DefaultODataSerializerProvider();
            HttpContext context = CreateHttpContext(serialierProvider);

            var pathHandler = new DefaultODataPathHandler(context.RequestServices);
            var path = pathHandler.Parse(model, "http://localhost/", uri);
            context.ODataFeature().Path = path;
            context.ODataFeature().Model = model;

            // Act
            var serializer = serialierProvider.GetODataPayloadSerializer(context, type);

            // Assert
            Assert.NotNull(serializer);
            var rawValueSerializer = Assert.IsType<ODataRawValueSerializer>(serializer);
            Assert.Equal(ODataPayloadKind.Value, rawValueSerializer.ODataPayloadKind);
        }

        [Theory]
        [InlineData(typeof(Product))]
        [InlineData(typeof(Address))]
        public void GetODataSerializer_ResourceSerializer_ForStructuralType(Type type)
        {
            // Arrange
            DefaultODataSerializerProvider serialierProvider = new DefaultODataSerializerProvider();
            HttpContext context = CreateHttpContext(serialierProvider);
            context.ODataFeature().Model = GetEdmModel();

            // Act
            var serializer = serialierProvider.GetODataPayloadSerializer(context, type);

            // Assert
            Assert.NotNull(serializer);
            var entitySerializer = Assert.IsType<ODataResourceSerializer>(serializer);
            Assert.Equal(entitySerializer.SerializerProvider, serialierProvider);
            Assert.Equal(entitySerializer.ODataPayloadKind, ODataPayloadKind.Resource);
        }

        [Theory]
        [InlineData(typeof(Product[]))]
        [InlineData(typeof(IEnumerable<Product>))]
        [InlineData(typeof(ICollection<Product>))]
        [InlineData(typeof(IList<Product>))]
        [InlineData(typeof(List<Product>))]
        [InlineData(typeof(PageResult<Product>))]
        [InlineData(typeof(Address[]))]
        [InlineData(typeof(IEnumerable<Address>))]
        [InlineData(typeof(ICollection<Address>))]
        [InlineData(typeof(IList<Address>))]
        [InlineData(typeof(List<Address>))]
        [InlineData(typeof(PageResult<Address>))]
        public void GetODataSerializer_ResourceSet_ForCollectionOfStructuralType(Type collectionType)
        {
            // Arrange
            DefaultODataSerializerProvider serialierProvider = new DefaultODataSerializerProvider();
            HttpContext context = CreateHttpContext(serialierProvider);
            context.ODataFeature().Model = GetEdmModel();

            // Act
            var serializer = serialierProvider.GetODataPayloadSerializer(context, collectionType);

            // Assert
            Assert.NotNull(serializer);
            var resourceSetSerializer = Assert.IsType<ODataResourceSetSerializer>(serializer);
            Assert.Equal(resourceSetSerializer.ODataPayloadKind, ODataPayloadKind.ResourceSet);
            Assert.Same(resourceSetSerializer.SerializerProvider, serialierProvider);
        }

        [Theory]
        [InlineData(typeof(ODataError), typeof(ODataErrorSerializer))]
        [InlineData(typeof(Uri), typeof(ODataEntityReferenceLinkSerializer))]
        [InlineData(typeof(ODataEntityReferenceLink), typeof(ODataEntityReferenceLinkSerializer))]
        [InlineData(typeof(Uri[]), typeof(ODataEntityReferenceLinksSerializer))]
        [InlineData(typeof(List<Uri>), typeof(ODataEntityReferenceLinksSerializer))]
        [InlineData(typeof(ODataEntityReferenceLinks), typeof(ODataEntityReferenceLinksSerializer))]
        public void GetODataSerializer_Returns_ExpectedSerializerType(Type payloadType, Type expectedSerializerType)
        {
            // Arrange
            DefaultODataSerializerProvider serialierProvider = new DefaultODataSerializerProvider();
            HttpContext context = CreateHttpContext(serialierProvider);

            // Act
            ODataSerializer serializer = serialierProvider.GetODataPayloadSerializer(context, payloadType);

            // Assert
            Assert.NotNull(serializer);
            Assert.IsType(expectedSerializerType, serializer);
        }

        [Fact]
        public void GetODataSerializer_ReturnsSameSerializer_ForSameType()
        {
            // Arrange
            DefaultODataSerializerProvider serialierProvider = new DefaultODataSerializerProvider();
            HttpContext context = CreateHttpContext(serialierProvider);

            // Act
            ODataSerializer firstSerializer = serialierProvider.GetODataPayloadSerializer(context, typeof(Product));
            ODataSerializer secondSerializer = serialierProvider.GetODataPayloadSerializer(context, typeof(Product));

            // Assert
            Assert.Same(firstSerializer, secondSerializer);
        }

        private static HttpContext CreateHttpContext(IODataSerializerProvider serializerProvider)
        {
            IServiceCollection services = new ServiceCollection();
            AddDefaultSerializers(services, serializerProvider);

            HttpContext context = new DefaultHttpContext();
            context.RequestServices = services.BuildServiceProvider();
            return context;
        }

        public static void AddDefaultSerializers(IServiceCollection services, IODataSerializerProvider serializerProvider)
        {
            // This is necessary to query the IOption<ODataOptions>
            services.AddOptions();

            // add the default OData lib services into service collection.
            IContainerBuilder builder = new DefaultContainerBuilder(services);
            builder.AddDefaultODataServices();

            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<ODataOptions>, ODataOptionsSetup>());

            services.TryAdd(ServiceDescriptor.Singleton(s => new ODataEnumSerializer(serializerProvider)));
            services.TryAdd(ServiceDescriptor.Singleton(s => new ODataResourceSerializer(serializerProvider)));
            services.TryAdd(ServiceDescriptor.Singleton(s => new ODataResourceSetSerializer(serializerProvider)));

            services.AddSingleton<ODataPrimitiveSerializer>();
            services.AddSingleton<ODataEntityReferenceLinkSerializer>();
            services.AddSingleton<ODataEntityReferenceLinksSerializer>();
            services.AddSingleton<ODataErrorSerializer>();
            services.AddSingleton<ODataRawValueSerializer>();
        }

        private static IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();

            // Enum type
            EdmEnumType enumType = new EdmEnumType("NS", "TestEnum");
            enumType.AddMember(new EdmEnumMember(enumType, "FirstValue", new EdmEnumMemberValue(0)));
            enumType.AddMember(new EdmEnumMember(enumType, "FirstValue", new EdmEnumMemberValue(1)));
            model.AddElement(enumType);
            model.SetAnnotationValue(model.FindDeclaredType("NS.TestEnum"), new ClrTypeAnnotation(typeof(TestEnum)));

            // Entity type
            EdmEntityType productType = new EdmEntityType("NS", "Product");
            productType.AddKeys(productType.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(productType);
            model.SetAnnotationValue(model.FindDeclaredType("NS.Product"), new ClrTypeAnnotation(typeof(Product)));

            // Complex type
            EdmComplexType address = new EdmComplexType("NS", "Address");
            address.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            model.AddElement(address);
            model.SetAnnotationValue(model.FindDeclaredType("NS.Address"), new ClrTypeAnnotation(typeof(Address)));

            return model;
        }

        private enum TestEnum
        {
            FirstValue,
            SecondValue
        }

        public class Product
        {
            public int Id { get; set; }
        }

        public class Address
        {
            public string City { get; set; }
        }
    }
}
