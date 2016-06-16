// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Serialization.Models;
using System.Web.OData.Query;
using System.Web.OData.TestCommon;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Formatter.Serialization
{
    public class ODataResourceSetSerializerTests
    {
        IEdmModel _model;
        IEdmEntitySet _customerSet;
        Customer[] _customers;
        ODataResourceSetSerializer _serializer;
        IEdmCollectionTypeReference _customersType;
        ODataSerializerContext _writeContext;

        public ODataResourceSetSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _customerSet = _model.EntityContainer.FindEntitySet("Customers");
            _model.SetAnnotationValue(_customerSet.EntityType(), new ClrTypeAnnotation(typeof(Customer)));
            _customers = new[] {
                new Customer()
                {
                    FirstName = "Foo",
                    LastName = "Bar",
                    ID = 10,
                },
                new Customer()
                {
                    FirstName = "Foo",
                    LastName = "Bar",
                    ID = 42,
                }
            };

            _customersType = _model.GetEdmTypeReference(typeof(Customer[])).AsCollection();

            _writeContext = new ODataSerializerContext() { NavigationSource = _customerSet, Model = _model };
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataResourceSetSerializer(serializerProvider: null),
                "serializerProvider");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: null, messageWriter: null, writeContext: new ODataSerializerContext()),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_WriteContext()
        {
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: null, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObject_Calls_WriteObjectInline()
        {
            // Arrange
            object graph = new object();
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer
                .Setup(s => s.WriteObjectInline(graph, It.Is<IEdmTypeReference>(e => _customersType.IsEquivalentTo(e)),
                    It.IsAny<ODataWriter>(), _writeContext))
                .Verifiable();

            // Act
            serializer.Object.WriteObject(graph, typeof(Customer[]), ODataTestUtil.GetMockODataMessageWriter(), _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_ThrowsArgumentNull_Writer()
        {
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObjectInline(graph: null, expectedType: null, writer: null, writeContext: new ODataSerializerContext()),
                "writer");
        }

        [Fact]
        public void WriteObjectInline_ThrowsArgumentNull_WriteContext()
        {
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObjectInline(graph: null, expectedType: null, writer: new Mock<ODataWriter>().Object, writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObjectInline_ThrowsSerializationException_CannotSerializerNull()
        {
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());
            Assert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(graph: null, expectedType: _customersType,
                    writer: new Mock<ODataWriter>().Object, writeContext: _writeContext),
                "Cannot serialize a null 'ResourceSet'.");
        }

        [Fact]
        public void WriteObjectInline_ThrowsSerializationException_IfGraphIsNotEnumerable()
        {
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());
            Assert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(graph: 42, expectedType: _customersType,
                    writer: new Mock<ODataWriter>().Object, writeContext: _writeContext),
                "ODataResourceSetSerializer cannot write an object of type 'System.Int32'.");
        }

        [Fact]
        public void WriteObjectInline_Throws_NullElementInCollection_IfResourceSetContainsNullElement()
        {
            // Arrange
            IEnumerable instance = new object[] { null };
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());

            // Act
            Assert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext),
                "Collections cannot contain null elements.");
        }

        [Fact]
        public void WriteObjectInline_Throws_TypeCannotBeSerialized_IfResourceSetContainsEntityThatCannotBeSerialized()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            HttpRequestMessage request = new HttpRequestMessage();
            serializerProvider.Setup(s => s.GetODataPayloadSerializer(_model, typeof(int), request)).Returns<ODataSerializer>(null);
            IEnumerable instance = new object[] { 42 };
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act
            Assert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext),
                "'Default.Customer' cannot be serialized using the ODataMediaTypeFormatter.");
        }

        [Fact]
        public void WriteObjectInline_Calls_CreateResourceSet()
        {
            // Arrange
            IEnumerable instance = new object[0];
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns(new ODataResourceSet()).Verifiable();

            // Act
            serializer.Object.WriteObjectInline(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Throws_CannotSerializerNull_IfCreateResourceSetReturnsNull()
        {
            // Arrange
            IEnumerable instance = new object[0];
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns<ODataResourceSet>(null);
            ODataWriter writer = new Mock<ODataWriter>().Object;

            // Act & Assert
            Assert.Throws<SerializationException>(
                () => serializer.Object.WriteObjectInline(instance, _customersType, writer, _writeContext),
                "Cannot serialize a null 'ResourceSet'.");
        }

        [Fact]
        public void WriteObjectInline_Writes_CreateResourceSetOutput()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataResourceSet resourceSet = new ODataResourceSet();
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns(resourceSet);
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer.Setup(s => s.WriteStart(resourceSet)).Verifiable();

            // Act
            serializer.Object.WriteObjectInline(instance, _customersType, writer.Object, _writeContext);

            // Assert
            writer.Verify();
        }

        [Fact]
        public void WriteObjectInline_WritesEachEntityInstance()
        {
            // Arrange
            Mock<ODataEdmTypeSerializer> customerSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Resource);
            ODataSerializerProvider provider = ODataTestUtil.GetMockODataSerializerProvider(customerSerializer.Object);
            var mockWriter = new Mock<ODataWriter>();

            customerSerializer.Setup(s => s.WriteObjectInline(_customers[0], _customersType.ElementType(), mockWriter.Object, _writeContext)).Verifiable();
            customerSerializer.Setup(s => s.WriteObjectInline(_customers[1], _customersType.ElementType(), mockWriter.Object, _writeContext)).Verifiable();

            _serializer = new ODataResourceSetSerializer(provider);

            // Act
            _serializer.WriteObjectInline(_customers, _customersType, mockWriter.Object, _writeContext);

            // Assert
            customerSerializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Can_WriteCollectionOfIEdmObjects()
        {
            // Arrange
            IEdmTypeReference edmType = new EdmEntityTypeReference(new EdmEntityType("NS", "Name"), isNullable: false);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(new EdmCollectionType(edmType));
            Mock<IEdmObject> edmObject = new Mock<IEdmObject>();
            edmObject.Setup(e => e.GetEdmType()).Returns(edmType);

            var mockWriter = new Mock<ODataWriter>();

            Mock<ODataEdmTypeSerializer> customSerializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Resource);
            customSerializer.Setup(s => s.WriteObjectInline(edmObject.Object, edmType, mockWriter.Object, _writeContext)).Verifiable();

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(edmType)).Returns(customSerializer.Object);

            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act
            serializer.WriteObjectInline(new[] { edmObject.Object }, collectionType, mockWriter.Object, _writeContext);

            // Assert
            customSerializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Sets_CountQueryOption_OnWriteStart()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataResourceSet resourceSet = new ODataResourceSet { Count = 1000 };
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns(resourceSet);
            var mockWriter = new Mock<ODataWriter>();

            mockWriter.Setup(m => m.WriteStart(It.Is<ODataResourceSet>(f => f.Count == 1000))).Verifiable();

            // Act
            serializer.Object.WriteObjectInline(instance, _customersType, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public void WriteObjectInline_Sets_NextPageLink_OnWriteEnd()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataResourceSet resourceSet = new ODataResourceSet { NextPageLink = new Uri("http://nextlink.com/") };
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns(resourceSet);
            var mockWriter = new Mock<ODataWriter>();

            mockWriter.Setup(m => m.WriteStart(It.Is<ODataResourceSet>(f => f.NextPageLink == null))).Verifiable();
            mockWriter
                .Setup(m => m.WriteEnd())
                .Callback(() =>
                {
                    Assert.Equal("http://nextlink.com/", resourceSet.NextPageLink.AbsoluteUri);
                })
                .Verifiable();

            // Act
            serializer.Object.WriteObjectInline(instance, _customersType, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public void CreateResource_Sets_CountValueForPageResult()
        {
            // Arrange
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());
            Uri expectedNextLink = new Uri("http://nextlink.com");
            const long ExpectedCountValue = 1000;

            var result = new PageResult<Customer>(_customers, expectedNextLink, ExpectedCountValue);

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, new ODataSerializerContext());

            // Assert
            Assert.Equal(ExpectedCountValue, resourceSet.Count);
        }

        [Fact]
        public void CreateResource_Sets_NextPageLinkForPageResult()
        {
            // Arrange
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());
            Uri expectedNextLink = new Uri("http://nextlink.com");
            const long ExpectedCountValue = 1000;

            var result = new PageResult<Customer>(_customers, expectedNextLink, ExpectedCountValue);

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, new ODataSerializerContext());

            // Assert
            Assert.Equal(expectedNextLink, resourceSet.NextPageLink);
        }

        [Fact]
        public void CreateResourceSet_Sets_CountValueFromContext()
        {
            // Arrange
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());
            const long ExpectedCountValue = 1000;
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().TotalCount = ExpectedCountValue;
            var result = new object[0];

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, new ODataSerializerContext { Request = request });

            // Assert
            Assert.Equal(ExpectedCountValue, resourceSet.Count);
        }

        [Fact]
        public void CreateResourceSet_Sets_NextPageLinkFromContext()
        {
            // Arrange
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());
            Uri expectedNextLink = new Uri("http://nextlink.com");
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().NextLink = expectedNextLink;
            var result = new object[0];

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, new ODataSerializerContext { Request = request });

            // Assert
            Assert.Equal(expectedNextLink, resourceSet.NextPageLink);
        }

        [Fact]
        public void CreateResource_Ignores_NextPageLink_ForInnerResourceSets()
        {
            // Arrange
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());
            Uri nextLink = new Uri("http://somelink");
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().NextLink = nextLink;
            var result = new object[0];
            IEdmNavigationProperty navProp = _customerSet.EntityType().NavigationProperties().First();
            SelectExpandClause selectExpandClause = new SelectExpandClause(new SelectItem[0], allSelected: true);
            ResourceContext entity = new ResourceContext
            {
                SerializerContext =
                    new ODataSerializerContext { Request = request, NavigationSource = _customerSet, Model = _model }
            };
            ODataSerializerContext nestedContext = new ODataSerializerContext(entity, selectExpandClause, navProp);

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, nestedContext);

            // Assert
            Assert.Null(resourceSet.NextPageLink);
        }

        [Fact]
        public void CreateResourceSet_Ignores_CountValue_ForInnerResourceSets()
        {
            // Arrange
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().TotalCount = 42;
            var result = new object[0];
            IEdmNavigationProperty navProp = _customerSet.EntityType().NavigationProperties().First();
            SelectExpandClause selectExpandClause = new SelectExpandClause(new SelectItem[0], allSelected: true);
            ResourceContext entity = new ResourceContext
            {
                SerializerContext =
                    new ODataSerializerContext { Request = request, NavigationSource = _customerSet, Model = _model }
            };
            ODataSerializerContext nestedContext = new ODataSerializerContext(entity, selectExpandClause, navProp);

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, nestedContext);

            // Assert
            Assert.Null(resourceSet.Count);
        }

        [Fact]
        public void CreateResourceSet_SetsNextPageLink_WhenWritingTruncatedCollection_ForExpandedProperties()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            IEdmCollectionTypeReference customersType = new EdmCollectionTypeReference(new EdmCollectionType(model.Customer.AsReference()));
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());
            SelectExpandClause selectExpandClause = new SelectExpandClause(new SelectItem[0], allSelected: true);
            IEdmNavigationProperty ordersProperty = model.Customer.NavigationProperties().First();
            ResourceContext entity = new ResourceContext
            {
                SerializerContext = new ODataSerializerContext { NavigationSource = model.Customers, Model = model.Model }
            };
            ODataSerializerContext nestedContext = new ODataSerializerContext(entity, selectExpandClause, ordersProperty);
            TruncatedCollection<Order> orders = new TruncatedCollection<Order>(new[] { new Order(), new Order() }, pageSize: 1);

            NavigationSourceLinkBuilderAnnotation linkBuilder = new NavigationSourceLinkBuilderAnnotation();
            linkBuilder.AddNavigationPropertyLinkBuilder(ordersProperty,
                new NavigationLinkBuilder((entityContext, navigationProperty) => new Uri("http://navigation-link/"),
                    false));

            model.Model.SetNavigationSourceLinkBuilder(model.Customers, linkBuilder);
            model.Model.SetNavigationSourceLinkBuilder(model.Orders, new NavigationSourceLinkBuilderAnnotation());

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(orders, _customersType, nestedContext);

            // Assert
            Assert.Equal("http://navigation-link/?$skip=1", resourceSet.NextPageLink.AbsoluteUri);
        }

        [Fact]
        public void CreateResourceSet_SetsODataOperations()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            IEdmCollectionTypeReference customersType = new EdmCollectionTypeReference(new EdmCollectionType(model.Customer.AsReference()));
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());
            ODataSerializerContext context = new ODataSerializerContext
            {
                NavigationSource = model.Customers,
                Request = new HttpRequestMessage(),
                Model = model.Model,
                MetadataLevel = ODataMetadataLevel.FullMetadata,
                Url = CreateMetadataLinkFactory("http://IgnoreMetadataPath")
            };

            var result = new object[0];

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, customersType, context);

            // Assert
            Assert.Equal(1, resourceSet.Actions.Count());
            Assert.Equal(2, resourceSet.Functions.Count());
        }

        [Theory]
        [InlineData(ODataMetadataLevel.MinimalMetadata)]
        [InlineData(ODataMetadataLevel.NoMetadata)]
        public void CreateODataOperation_OmitsOperations_WhenNonFullMetadata(ODataMetadataLevel metadataLevel)
        {
            // Arrange
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());

            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmFunction function = new EdmFunction("NS", "Function", returnType, isBound: true, entitySetPathExpression: null, isComposable: false);

            ResourceSetContext resourceSetContext = new ResourceSetContext();
            ODataSerializerContext serializerContext = new ODataSerializerContext
            {
                MetadataLevel = metadataLevel
            };
            // Act

            ODataOperation operation = serializer.CreateODataOperation(function, resourceSetContext, serializerContext);

            // Assert
            Assert.Null(operation);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateODataOperations_CreateOperations(bool followConventions)
        {
            // Arrange
            // Arrange
            string expectedTarget = "aa://Target";
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(new DefaultODataSerializerProvider());
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<FeedCustomer>("Customers");
            var function = builder.EntityType<FeedCustomer>().Collection.Function("MyFunction").Returns<int>();
            function.HasFeedFunctionLink(a => new Uri(expectedTarget), followConventions);
            IEdmModel model = builder.GetEdmModel();

            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            IEdmFunction edmFunction = model.SchemaElements.OfType<IEdmFunction>().First(f => f.Name == "MyFunction");
            string expectedMetadataPrefix = "http://Metadata";

            UrlHelper url = CreateMetadataLinkFactory(expectedMetadataPrefix);
            HttpRequestMessage request = new HttpRequestMessage();
            ResourceSetContext resourceSetContext = new ResourceSetContext
            {
                EntitySetBase = customers,
                Request = request,
                Url = url
            };

            ODataSerializerContext serializerContext = new ODataSerializerContext
            {
                NavigationSource = customers,
                Request = request,
                Model = model,
                MetadataLevel = ODataMetadataLevel.FullMetadata,
                Url = url
            };

            // Act
            ODataOperation actualOperation = serializer.CreateODataOperation(edmFunction, resourceSetContext, serializerContext);

            // Assert
            Assert.NotNull(actualOperation);
            string expectedMetadata = expectedMetadataPrefix + "#Default.MyFunction";
            ODataOperation expectedFunction = new ODataFunction
            {
                Metadata = new Uri(expectedMetadata),
                Target = new Uri(expectedTarget),
                Title = "MyFunction"
            };

            AssertEqual(expectedFunction, actualOperation);
        }

        private static void AssertEqual(ODataOperation expected, ODataOperation actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);
            AssertEqual(expected.Metadata, actual.Metadata);
            AssertEqual(expected.Target, actual.Target);
            Assert.Equal(expected.Title, actual.Title);
        }

        private static void AssertEqual(Uri expected, Uri actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }

            Assert.NotNull(actual);
            Assert.Equal(expected.AbsoluteUri, actual.AbsoluteUri);
        }

        private static UrlHelper CreateMetadataLinkFactory(string metadataPath)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, metadataPath);
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.SetFakeRootContainer();
            configuration.Routes.MapFakeODataRoute();
            request.SetConfiguration(configuration);
            request.SetFakeRequestContainer();
            request.SetFakeODataRouteName();
            return new UrlHelper(request);
        }

        public class FeedCustomer
        {
            public int Id { get; set; }
        }
    }
}
