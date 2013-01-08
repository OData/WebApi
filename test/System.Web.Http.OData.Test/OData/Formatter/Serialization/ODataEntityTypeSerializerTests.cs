// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataEntityTypeSerializerTests
    {
        IEdmModel _model;
        IEdmEntitySet _customerSet;
        Customer _customer;
        ODataEntityTypeSerializer _serializer;
        UrlHelper _urlHelper;
        ODataSerializerContext _writeContext;

        public ODataEntityTypeSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();

            _model.SetAnnotationValue<ClrTypeAnnotation>(_model.FindType("Default.Customer"), new ClrTypeAnnotation(typeof(Customer)));
            _model.SetAnnotationValue<ClrTypeAnnotation>(_model.FindType("Default.Order"), new ClrTypeAnnotation(typeof(Order)));

            _customerSet = _model.FindDeclaredEntityContainer("Default.Container").FindEntitySet("Customers");
            _customer = new Customer()
            {
                FirstName = "Foo",
                LastName = "Bar",
                ID = 10,
            };

            ODataSerializerProvider serializerProvider = new DefaultODataSerializerProvider(_model);
            _serializer = new ODataEntityTypeSerializer(
                new EdmEntityTypeReference(_customerSet.ElementType, isNullable: false),
                serializerProvider);
            _urlHelper = new Mock<UrlHelper>(new HttpRequestMessage()).Object;
            _writeContext = new ODataSerializerContext() { EntitySet = _customerSet, UrlHelper = _urlHelper };
        }

        [Fact]
        public void WriteObjectInline_UsesCorrectTypeName()
        {
            // Arrange & Assert
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer
                .Setup(w => w.WriteStart(It.IsAny<ODataEntry>()))
                .Callback((ODataEntry entry) =>
                {
                    Assert.Equal("Default.Customer", entry.TypeName);
                });

            // Act
            _serializer.WriteObjectInline(_customer, writer.Object, _writeContext);
        }

        [Fact]
        public void WriteObjectInline_WritesCorrectIdLink()
        {
            // Arrange
            bool customIdLinkbuilderCalled = false;
            EntitySetLinkBuilderAnnotation linkAnnotation = new MockEntitySetLinkBuilderAnnotation
            {
                IdLinkBuilder = new SelfLinkBuilder<string>((EntityInstanceContext context) =>
                {
                    Assert.Equal(context.EdmModel, _model);
                    Assert.Equal(context.EntityInstance, _customer);
                    Assert.Equal(context.EntitySet, _customerSet);
                    Assert.Equal(context.EntityType, _customerSet.ElementType);
                    customIdLinkbuilderCalled = true;
                    return "http://sample_id_link";
                },
                followsConventions: false)
            };
            _model.SetEntitySetLinkBuilderAnnotation(_customerSet, linkAnnotation);

            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer
                .Setup(w => w.WriteStart(It.IsAny<ODataEntry>()))
                .Callback((ODataEntry entry) =>
                {
                    Assert.Equal(entry.Id, "http://sample_id_link");
                });

            // Act
            _serializer.WriteObjectInline(_customer, writer.Object, _writeContext);

            // Assert
            Assert.True(customIdLinkbuilderCalled);
        }

        [Fact]
        public void WriteObjectInline_WritesCorrectEditLink()
        {
            // Arrange
            bool customEditLinkbuilderCalled = false;
            EntitySetLinkBuilderAnnotation linkAnnotation = new MockEntitySetLinkBuilderAnnotation
            {
                EditLinkBuilder = new SelfLinkBuilder<Uri>((EntityInstanceContext context) =>
                {
                    Assert.Equal(context.EdmModel, _model);
                    Assert.Equal(context.EntityInstance, _customer);
                    Assert.Equal(context.EntitySet, _customerSet);
                    Assert.Equal(context.EntityType, _customerSet.ElementType);
                    customEditLinkbuilderCalled = true;
                    return new Uri("http://sample_edit_link");
                },
                followsConventions: false)
            };
            _model.SetEntitySetLinkBuilderAnnotation(_customerSet, linkAnnotation);

            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer
                .Setup(w => w.WriteStart(It.IsAny<ODataEntry>()))
                .Callback((ODataEntry entry) =>
                {
                    Assert.Equal(entry.EditLink, new Uri("http://sample_edit_link"));
                });

            // Act
            _serializer.WriteObjectInline(_customer, writer.Object, _writeContext);

            // Assert
            Assert.True(customEditLinkbuilderCalled);
        }

        [Fact]
        public void WriteObjectInline_WritesCorrectReadLink()
        {
            // Arrange
            bool customReadLinkbuilderCalled = false;
            EntitySetLinkBuilderAnnotation linkAnnotation = new MockEntitySetLinkBuilderAnnotation
            {
                ReadLinkBuilder = new SelfLinkBuilder<Uri>((EntityInstanceContext context) =>
                {
                    Assert.Equal(context.EdmModel, _model);
                    Assert.Equal(context.EntityInstance, _customer);
                    Assert.Equal(context.EntitySet, _customerSet);
                    Assert.Equal(context.EntityType, _customerSet.ElementType);
                    customReadLinkbuilderCalled = true;
                    return new Uri("http://sample_read_link");
                },
                followsConventions: false)
            };

            _model.SetEntitySetLinkBuilderAnnotation(_customerSet, linkAnnotation);

            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer
                .Setup(w => w.WriteStart(It.IsAny<ODataEntry>()))
                .Callback((ODataEntry entry) =>
                {
                    Assert.Equal(entry.ReadLink, new Uri("http://sample_read_link"));
                });

            // Act
            _serializer.WriteObjectInline(_customer, writer.Object, _writeContext);

            // Assert
            Assert.True(customReadLinkbuilderCalled);
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_DoesNotAddAnnotation_InDefaultMetadataMode()
        {
            // Arrange
            ODataEntry entry = new ODataEntry();

            // Act
            ODataEntityTypeSerializer.AddTypeNameAnnotationAsNeeded(entry, null, ODataMetadataLevel.Default);

            // Assert
            Assert.Null(entry.GetAnnotation<SerializationTypeNameAnnotation>());
        }

        [Fact]
        public void AddTypeNameAnnotationAsNeeded_AddsAnnotation_InJsonLightMetadataMode()
        {
            // Arrange
            string expectedTypeName = "TypeName";
            ODataEntry entry = new ODataEntry
            {
                TypeName = expectedTypeName
            };

            // Act
            ODataEntityTypeSerializer.AddTypeNameAnnotationAsNeeded(entry, null, ODataMetadataLevel.MinimalMetadata);

            // Assert
            SerializationTypeNameAnnotation annotation = entry.GetAnnotation<SerializationTypeNameAnnotation>();
            Assert.NotNull(annotation); // Guard
            Assert.Equal(expectedTypeName, annotation.TypeName);
        }

        [Theory]
        [InlineData(ODataMetadataLevel.Default, false)]
        [InlineData(ODataMetadataLevel.FullMetadata, false)]
        [InlineData(ODataMetadataLevel.MinimalMetadata, true)]
        [InlineData(ODataMetadataLevel.NoMetadata, true)]
        public void ShouldAddTypeNameAnnotation(ODataMetadataLevel metadataLevel, bool expectedResult)
        {
            // Act
            bool actualResult = ODataEntityTypeSerializer.ShouldAddTypeNameAnnotation(metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("MatchingType", "MatchingType", ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData("DoesNotMatch1", "DoesNotMatch2", ODataMetadataLevel.MinimalMetadata, false)]
        [InlineData("IgnoredEntryType", "IgnoredEntitySetType", ODataMetadataLevel.NoMetadata, true)]
        public void ShouldSuppressTypeNameSerialization(string entryType, string entitySetType,
            ODataMetadataLevel metadataLevel, bool expectedResult)
        {
            // Arrange
            ODataEntry entry = new ODataEntry
            {
                TypeName = entryType
            };
            IEdmEntitySet entitySet = CreateEntitySetWithElementTypeName(entitySetType);

            // Act
            bool actualResult = ODataEntityTypeSerializer.ShouldSuppressTypeNameSerialization(entry, null, metadataLevel);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        private static IEdmEntitySet CreateEntitySetWithElementTypeName(string typeName)
        {
            Mock<IEdmEntityType> entityTypeMock = new Mock<IEdmEntityType>();
            entityTypeMock.Setup<string>(o => o.Name).Returns(typeName);
            IEdmEntityType entityType = entityTypeMock.Object;
            Mock<IEdmEntitySet> entitySetMock = new Mock<IEdmEntitySet>();
            entitySetMock.Setup<IEdmEntityType>(o => o.ElementType).Returns(entityType);
            return entitySetMock.Object;
        }

        private class Customer
        {
            public Customer()
            {
                this.Orders = new List<Order>();
            }
            public int ID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public IList<Order> Orders { get; private set; }
        }

        private class Order
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public Customer Customer { get; set; }
        }
    }
}
