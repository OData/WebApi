//-----------------------------------------------------------------------------
// <copyright file="ODataResourceSetSerializerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.AspNet.OData.Test.Formatter.Serialization.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;
#else
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.AspNet.OData.Test.Formatter.Serialization.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    public class ODataResourceSetSerializerTests
    {
        IEdmModel _model;
        IEdmEntitySet _customerSet;
        Customer[] _customers;
        ODataResourceSetSerializer _serializer;
        IEdmCollectionTypeReference _customersType;
        IEdmCollectionTypeReference _addressesType;
        ODataSerializerContext _writeContext;
        ODataSerializerProvider _serializerProvider;

        public ODataResourceSetSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _customerSet = _model.EntityContainer.FindEntitySet("Customers");
            IEdmComplexType addressType = _model.SchemaElements.OfType<IEdmComplexType>()
                .First(c => c.Name == "Address");
            _model.SetAnnotationValue(_customerSet.EntityType(), new ClrTypeAnnotation(typeof(Customer)));
            _model.SetAnnotationValue(addressType, new ClrTypeAnnotation(typeof(Address)));
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
            _addressesType = _model.GetEdmTypeReference(typeof(Address[])).AsCollection();
            _writeContext = new ODataSerializerContext() { NavigationSource = _customerSet, Model = _model };
            _serializerProvider = ODataSerializerProviderFactory.Create();
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataResourceSetSerializer(serializerProvider: null),
                "serializerProvider");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: null, messageWriter: null, writeContext: new ODataSerializerContext()),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_WriteContext()
        {
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: null, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObject_Calls_WriteObjectInline()
        {
            // Arrange
            object graph = new object();
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(_serializerProvider);
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
        public void WriteObject_CanWriteTopLevelResourceSetContainsNullComplexElement()
        {
            // Arrange
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);

            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/"), }
            };
            settings.SetContentType(ODataFormat.Json);

            ODataMessageWriter writer = new ODataMessageWriter(message, settings);
            IList<Address> addresses = new[]
            {
                new Address { City = "Redmond" },
                null,
                new Address { City = "Shanghai" }
            };

            ODataSerializerContext writeContext = new ODataSerializerContext { Model = _model };

            // Act
            serializer.WriteObject(addresses, typeof(IList<Address>), writer, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            string result = new StreamReader(stream).ReadToEnd();

            // Assert
            Assert.Equal("{\"@odata.context\":\"http://any/$metadata#Collection(Default.Address)\"," +
                "\"value\":[" +
                  "{\"Street\":null,\"City\":\"Redmond\",\"State\":null,\"CountryOrRegion\":null,\"ZipCode\":null}," +
                  "null," +
                  "{\"Street\":null,\"City\":\"Shanghai\",\"State\":null,\"CountryOrRegion\":null,\"ZipCode\":null}" +
                  "]}", result);
        }

        [Fact]
        public void WriteObject_CanWrite_TopLevelResourceSet_ContainsEmptyCollectionOfDynamicComplexElement()
        {
            // Arrange
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);

            ODataMessageWriterSettings settings = new ODataMessageWriterSettings
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/"), }
            };
            settings.SetContentType(ODataFormat.Json);

            ODataMessageWriter writer = new ODataMessageWriter(message, settings);
            IList<SimpleOpenAddress> addresses = new[]
            {
                new SimpleOpenAddress
                {
                    City = "Redmond",
                    Street = "Microsoft Rd",
                    Properties = new Dictionary<string, object>
                    {
                        { "StringProp", "abc" },
                        { "Locations", new SimpleOpenAddress[] {} } // empty collection of complex
                    }
                }
            };

            var builder = ODataConventionModelBuilderFactory.Create();
            builder.ComplexType<SimpleOpenAddress>();
            IEdmModel model = builder.GetEdmModel();
            ODataSerializerContext writeContext = new ODataSerializerContext { Model = model };

            // Act
            serializer.WriteObject(addresses, typeof(IList<SimpleOpenAddress>), writer, writeContext);
            stream.Seek(0, SeekOrigin.Begin);
            string result = JObject.Parse(new StreamReader(stream).ReadToEnd()).ToString();

            // Assert
            Assert.Equal(@"{
  ""@odata.context"": ""http://any/$metadata#Collection(Microsoft.AspNet.OData.Test.Common.SimpleOpenAddress)"",
  ""value"": [
    {
      ""Street"": ""Microsoft Rd"",
      ""City"": ""Redmond"",
      ""StringProp"": ""abc"",
      ""Locations@odata.type"": ""#Collection(Microsoft.AspNet.OData.Test.Common.SimpleOpenAddress)"",
      ""Locations"": []
    }
  ]
}", result);
        }

        [Fact]
        public void WriteObjectInline_ThrowsArgumentNull_Writer()
        {
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObjectInline(graph: null, expectedType: null, writer: null, writeContext: new ODataSerializerContext()),
                "writer");
        }

        [Fact]
        public void WriteObjectInline_ThrowsArgumentNull_WriteContext()
        {
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObjectInline(graph: null, expectedType: null, writer: new Mock<ODataWriter>().Object, writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObjectInline_ThrowsSerializationException_CannotSerializerNull()
        {
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(graph: null, expectedType: _customersType,
                    writer: new Mock<ODataWriter>().Object, writeContext: _writeContext),
                "Cannot serialize a null 'ResourceSet'.");
        }

        [Fact]
        public void WriteObjectInline_ThrowsSerializationException_IfGraphIsNotEnumerable()
        {
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(graph: 42, expectedType: _customersType,
                    writer: new Mock<ODataWriter>().Object, writeContext: _writeContext),
                "ODataResourceSetSerializer cannot write an object of type 'System.Int32'.");
        }

        [Fact]
        public void WriteObjectInline_Throws_NullElementInCollection_IfResourceSetContainsNullElement()
        {
            // Arrange
            IEnumerable instance = new object[] { null };
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext),
                "Collections cannot contain null elements.");
        }

        [Fact]
        public void WriteObjectInline_DoesnotThrow_NullElementInCollection_IfResourceSetContainsNullComplexElement()
        {
            // Arrange
            IEnumerable instance = new object[] { null };
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            ODataSerializerContext writeContext = new ODataSerializerContext { NavigationSource = null, Model = _model };

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => serializer.WriteObjectInline(instance, _addressesType, new Mock<ODataWriter>().Object, writeContext));
        }

        [Fact]
        public void WriteObjectInline_Throws_TypeCannotBeSerialized_IfResourceSetContainsEntityThatCannotBeSerialized()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            var request = RequestFactory.Create();
            serializerProvider.Setup(s => s.GetODataPayloadSerializer(typeof(int), request)).Returns<ODataSerializer>(null);
            IEnumerable instance = new object[] { 42 };
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(serializerProvider.Object);

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteObjectInline(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext),
                "'Default.Customer' cannot be serialized using the ODataMediaTypeFormatter.");
        }

        [Fact]
        public void WriteObjectInline_Calls_CreateResourceSet()
        {
            // Arrange
            IEnumerable instance = new object[0];
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(_serializerProvider);
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
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(_serializerProvider);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateResourceSet(instance, _customersType, _writeContext)).Returns<ODataResourceSet>(null);
            ODataWriter writer = new Mock<ODataWriter>().Object;

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.Object.WriteObjectInline(instance, _customersType, writer, _writeContext),
                "Cannot serialize a null 'ResourceSet'.");
        }

        [Fact]
        public void WriteObjectInline_Writes_CreateResourceSetOutput()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataResourceSet resourceSet = new ODataResourceSet();
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(_serializerProvider);
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
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(_serializerProvider);
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
            Mock<ODataResourceSetSerializer> serializer = new Mock<ODataResourceSetSerializer>(_serializerProvider);
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
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
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
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
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
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            const long ExpectedCountValue = 1000;
            var request = RequestFactory.Create();
            request.ODataContext().TotalCount = ExpectedCountValue;
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
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            Uri expectedNextLink = new Uri("http://nextlink.com");
            var request = RequestFactory.Create();
            request.ODataContext().NextLink = expectedNextLink;
            var result = new object[0];

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, new ODataSerializerContext { Request = request });

            // Assert
            Assert.Equal(expectedNextLink, resourceSet.NextPageLink);
        }

        [Fact]
        public void CreateODataFeed_Sets_DeltaLinkFromContext()
        {
            // Arrange
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            Uri expectedDeltaLink = new Uri("http://deltalink.com");
            var request = RequestFactory.Create();
            request.ODataContext().DeltaLink = expectedDeltaLink;
            var result = new object[0];

            // Act
            ODataResourceSet feed = serializer.CreateResourceSet(result, _customersType, new ODataSerializerContext { Request = request });

            // Assert
            Assert.Equal(expectedDeltaLink, feed.DeltaLink);
        }

        [Fact]
        public void CreateResource_Ignores_NextPageLink_ForInnerResourceSets()
        {
            // Arrange
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            Uri nextLink = new Uri("http://somelink");
            var request = RequestFactory.Create();
            request.ODataContext().NextLink = nextLink;
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
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            var request = RequestFactory.Create();
            request.ODataContext().TotalCount = 42;
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
        public void CreateResourceSet_SetsODataOperations()
        {
            // Arrange
            var config = RoutingConfigurationFactory.CreateWithRootContainer("OData");
            var request = RequestFactory.Create(config, "OData");
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            IEdmCollectionTypeReference customersType = new EdmCollectionTypeReference(new EdmCollectionType(model.Customer.AsReference()));
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            ODataSerializerContext context = new ODataSerializerContext
            {
                NavigationSource = model.Customers,
                Request = request,
                Model = model.Model,
                MetadataLevel = ODataMetadataLevel.FullMetadata,
                Url = CreateMetadataLinkFactory("http://IgnoreMetadataPath", request)
            };

            var result = new object[0];

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, customersType, context);

            // Assert
            Assert.Single(resourceSet.Actions);
            Assert.Equal(3, resourceSet.Functions.Count());
        }

        [Fact]
        public void SetODataFeatureTotalCountValueNull()
        {
            // Arrange
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            var request = RequestFactory.Create();
            request.ODataContext().TotalCount = null;

            var result = new object[0];

            // Act
            ODataResourceSet resourceSet = serializer.CreateResourceSet(result, _customersType, new ODataSerializerContext { Request = request });

            // Assert
            Assert.Null(resourceSet.Count);
        }

        [Theory]
        [InlineData(ODataMetadataLevel.MinimalMetadata)]
        [InlineData(ODataMetadataLevel.NoMetadata)]
        public void CreateODataOperation_OmitsOperations_WhenNonFullMetadata(ODataMetadataLevel metadataLevel)
        {
            // Arrange
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);

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
            ODataResourceSetSerializer serializer = new ODataResourceSetSerializer(_serializerProvider);
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<FeedCustomer>("Customers");
            var function = builder.EntityType<FeedCustomer>().Collection.Function("MyFunction").Returns<int>();
            function.HasFeedFunctionLink(a => new Uri(expectedTarget), followConventions);
            IEdmModel model = builder.GetEdmModel();

            IEdmEntitySet customers = model.EntityContainer.FindEntitySet("Customers");
            IEdmFunction edmFunction = model.SchemaElements.OfType<IEdmFunction>().First(f => f.Name == "MyFunction");
            string expectedMetadataPrefix = "http://Metadata";

            var request = RequestFactory.Create();
            ResourceSetContext resourceSetContext = new ResourceSetContext
            {
                EntitySetBase = customers,
                Request = request,
                Url = CreateMetadataLinkFactory(expectedMetadataPrefix, request)
            };

            ODataSerializerContext serializerContext = new ODataSerializerContext
            {
                NavigationSource = customers,
                Request = request,
                Model = model,
                MetadataLevel = ODataMetadataLevel.FullMetadata,
                Url = resourceSetContext.Url
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

#if NETCORE
        public class MyUrlHelper : IUrlHelper
        {
            private string _link;

            public MyUrlHelper(string metadataPath, ActionContext context)
            {
                _link = metadataPath;
                ActionContext = context;
            }

            public ActionContext ActionContext { get; set; }

            public string Action(UrlActionContext actionContext) => throw new NotImplementedException();

            public string Content(string contentPath) => throw new NotImplementedException();

            public bool IsLocalUrl(string url) => throw new NotImplementedException();

            public string Link(string routeName, object values) => _link;

            public string RouteUrl(UrlRouteContext routeContext) => throw new NotImplementedException();
        }

        private static IUrlHelper CreateMetadataLinkFactory(string metadataPath, HttpRequest request)
        {
            ActionContext context = new ActionContext();
            context.HttpContext = request.HttpContext;
            return new MyUrlHelper(metadataPath, context);
        }
#else
        private static UrlHelper CreateMetadataLinkFactory(string metadataPath, HttpRequestMessage notUsedRequest)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, metadataPath);
            request.EnableODataDependencyInjectionSupport();
            request.GetConfiguration().Routes.MapFakeODataRoute();
            return new UrlHelper(request);
        }
#endif
        public class FeedCustomer
        {
            public int Id { get; set; }
        }
    }
}
