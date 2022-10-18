//-----------------------------------------------------------------------------
// <copyright file="ODataDeltaFeedSerializerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Formatter.Serialization.Models;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization
{
    public class ODataDeltaFeedSerializerTests
    {
        IEdmModel _model;
        IEdmEntitySet _customerSet;
        Customer[] _customers;
        EdmChangedObjectCollection _deltaFeedCustomers;
        ODataDeltaFeedSerializer _serializer;
        IEdmCollectionTypeReference _customersType;
        private ODataPath _path;
        ODataSerializerContext _writeContext;
        ODataSerializerProvider _serializerProvider;

        public ODataDeltaFeedSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _customerSet = _model.EntityContainer.FindEntitySet("Customers");
            _model.SetAnnotationValue(_customerSet.EntityType(), new ClrTypeAnnotation(typeof(Customer)));
            _path = new ODataPath(new EntitySetSegment(_customerSet));
            _customers = new[] {
                new Customer()
                {
                    FirstName = "Foo",
                    LastName = "Bar",
                    ID = 10,
                    HomeAddress = new Address()
                    {
                        Street = "Street",
                        ZipCode = null,
                    }
                },
                new Customer()
                {
                    FirstName = "Foo",
                    LastName = "Bar",
                    ID = 42,
                }
            };

            _deltaFeedCustomers = new EdmChangedObjectCollection(_customerSet.EntityType());
            EdmDeltaEntityObject newCustomer = new EdmDeltaEntityObject(_customerSet.EntityType());
            newCustomer.TrySetPropertyValue("ID", 10);
            newCustomer.TrySetPropertyValue("FirstName", "Foo");
            EdmDeltaComplexObject newCustomerAddress = new EdmDeltaComplexObject(_model.FindType("Default.Address") as IEdmComplexType);
            newCustomerAddress.TrySetPropertyValue("Street", "Street");
            newCustomerAddress.TrySetPropertyValue("ZipCode", null);
            newCustomer.TrySetPropertyValue("HomeAddress", newCustomerAddress);
            _deltaFeedCustomers.Add(newCustomer);

            _customersType = _model.GetEdmTypeReference(typeof(Customer[])).AsCollection();

            _writeContext = new ODataSerializerContext() { NavigationSource = _customerSet, Model = _model, Path = _path };
            _serializerProvider = ODataSerializerProviderFactory.Create();
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataDeltaFeedSerializer(serializerProvider: null),
                "serializerProvider");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(_serializerProvider);
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: null, messageWriter: null, writeContext: new ODataSerializerContext()),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_WriteContext()
        {
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(_serializerProvider);
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: null, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObject_ThrowsEntitySetMissingDuringSerialization()
        {
            object graph = new object();
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(_serializerProvider);
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteObject(graph: graph, type: null, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: new ODataSerializerContext()),
                "The related entity set could not be found from the OData path. The related entity set is required to serialize the payload.");
        }

        [Fact]
        public void WriteObject_Calls_WriteDeltaFeedInline()
        {
            // Arrange
            object graph = new object();
            Mock<ODataDeltaFeedSerializer> serializer = new Mock<ODataDeltaFeedSerializer>(_serializerProvider);
            serializer.CallBase = true;
            serializer
                .Setup(s => s.WriteDeltaFeedInline(graph, It.Is<IEdmTypeReference>(e => _customersType.IsEquivalentTo(e)),
                    It.IsAny<ODataWriter>(), _writeContext))
                .Verifiable();
            MemoryStream stream = new MemoryStream();
            IODataResponseMessage message = new ODataMessageWrapper(stream);
            ODataMessageWriterSettings settings = new ODataMessageWriterSettings()
            {
                ODataUri = new ODataUri { ServiceRoot = new Uri("http://any/") }
            };
            ODataMessageWriter messageWriter = new ODataMessageWriter(message, settings, _model);

            // Act
            serializer.Object.WriteObject(graph, typeof(Customer[]), messageWriter, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteDeltaFeedInline_ThrowsArgumentNull_Writer()
        {
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(_serializerProvider);
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteDeltaFeedInline(graph: null, expectedType: null, writer: null, writeContext: new ODataSerializerContext()),
                "writer");
        }

        [Fact]
        public void WriteDeltaFeedInline_ThrowsArgumentNull_WriteContext()
        {
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(_serializerProvider);
            ExceptionAssert.ThrowsArgumentNull(
                () => serializer.WriteDeltaFeedInline(graph: null, expectedType: null, writer: new Mock<ODataWriter>().Object, writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteDeltaFeedInline_ThrowsSerializationException_CannotSerializerNull()
        {
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(_serializerProvider);
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteDeltaFeedInline(graph: null, expectedType: _customersType,
                    writer: new Mock<ODataWriter>().Object, writeContext: _writeContext),
                "Cannot serialize a null 'deltafeed'.");
        }

        [Fact]
        public void WriteDeltaFeedInline_ThrowsSerializationException_IfGraphIsNotEnumerable()
        {
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(_serializerProvider);
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteDeltaFeedInline(graph: 42, expectedType: _customersType,
                    writer: new Mock<ODataWriter>().Object, writeContext: _writeContext),
                "ODataDeltaFeedSerializer cannot write an object of type 'System.Int32'.");
        }

        [Fact]
        public void WriteDeltaFeedInline_Throws_NullElementInCollection_IfFeedContainsNullElement()
        {
            // Arrange
            IEnumerable instance = new object[] { null };
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(_serializerProvider);

            // Act
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteDeltaFeedInline(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext),
                "Collections cannot contain null elements.");
        }

        [Fact]
        public void WriteDeltaFeedInline_Throws_TypeCannotBeSerialized_IfFeedContainsEntityThatCannotBeSerialized()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            var request = RequestFactory.Create();
            serializerProvider.Setup(s => s.GetODataPayloadSerializer(typeof(int), request)).Returns<ODataSerializer>(null);
            IEnumerable instance = new object[] { 42 };
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(serializerProvider.Object);

            // Act
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.WriteDeltaFeedInline(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext),
                "ODataDeltaFeedSerializer cannot write an object of type 'System.Object[]'.");
        }

        [Fact]
        public void WriteDeltaFeedInline_Calls_CreateODataDeltaFeed()
        {
            // Arrange
            IEnumerable instance = new object[0];
            Mock<ODataDeltaFeedSerializer> serializer = new Mock<ODataDeltaFeedSerializer>(_serializerProvider);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataDeltaFeed(instance, _customersType, _writeContext)).Returns(new ODataDeltaResourceSet()).Verifiable();

            // Act
            serializer.Object.WriteDeltaFeedInline(instance, _customersType, new Mock<ODataWriter>().Object, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteDeltaFeedInline_Throws_CannotSerializerNull_IfCreateODataDeltaFeedReturnsNull()
        {
            // Arrange
            IEnumerable instance = new object[0];
            Mock<ODataDeltaFeedSerializer> serializer = new Mock<ODataDeltaFeedSerializer>(_serializerProvider);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataDeltaFeed(instance, _customersType, _writeContext)).Returns<ODataDeltaResourceSet>(null);
            ODataWriter writer = new Mock<ODataWriter>().Object;

            // Act & Assert
            ExceptionAssert.Throws<SerializationException>(
                () => serializer.Object.WriteDeltaFeedInline(instance, _customersType, writer, _writeContext),
                "Cannot serialize a null 'deltafeed'.");
        }

        [Fact]
        public void WriteDeltaFeedInline_Writes_CreateODataFeedOutput()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataDeltaResourceSet deltafeed = new ODataDeltaResourceSet();
            Mock<ODataDeltaFeedSerializer> serializer = new Mock<ODataDeltaFeedSerializer>(_serializerProvider);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataDeltaFeed(instance, _customersType, _writeContext)).Returns(deltafeed);
            Mock<ODataWriter> writer = new Mock<ODataWriter>();
            writer.Setup(s => s.WriteStart(deltafeed)).Verifiable();

            // Act
            serializer.Object.WriteDeltaFeedInline(instance, _customersType, writer.Object, _writeContext);

            // Assert
            writer.Verify();
        }

        [Fact]
        public void WriteDeltaFeedInline_Can_WriteCollectionOfIEdmChangedObjects()
        {
            // Arrange
            IEdmTypeReference edmType = new EdmEntityTypeReference(new EdmEntityType("NS", "Name"), isNullable: false);
            IEdmCollectionTypeReference feedType = new EdmCollectionTypeReference(new EdmCollectionType(edmType));
            Mock<IEdmChangedObject> edmObject = new Mock<IEdmChangedObject>();
            edmObject.Setup(e => e.GetEdmType()).Returns(edmType);

            var mockWriter = new Mock<ODataWriter>();

            Mock<ODataResourceSerializer> customerSerializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            customerSerializer.Setup(s => s.WriteDeltaObjectInline(edmObject.Object, edmType, mockWriter.Object, _writeContext)).Verifiable();

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(edmType)).Returns(customerSerializer.Object);

            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(serializerProvider.Object);
            _writeContext.Type = typeof(IEdmObject);

            // Act
            serializer.WriteDeltaFeedInline(new[] { edmObject.Object }, feedType, mockWriter.Object, _writeContext);

            // Assert
            customerSerializer.Verify();
        }

        [Fact]
        public void WriteDeltaFeedInline_WritesEachEntityInstance()
        {
            // Arrange
            Mock<ODataResourceSerializer> customerSerializer = new Mock<ODataResourceSerializer>(_serializerProvider);
            ODataSerializerProvider provider = ODataTestUtil.GetMockODataSerializerProvider(customerSerializer.Object);
            var mockWriter = new Mock<ODataWriter>();
            customerSerializer.Setup(s => s.WriteDeltaObjectInline(_deltaFeedCustomers[0], _customersType.ElementType(), mockWriter.Object, _writeContext)).Verifiable();
            _serializer = new ODataDeltaFeedSerializer(provider);
            _writeContext.Type = typeof(IEdmObject);

            // Act
            _serializer.WriteDeltaFeedInline(_deltaFeedCustomers, _customersType, mockWriter.Object, _writeContext);

            // Assert
            customerSerializer.Verify();
        }

        [Fact]
        public void WriteDeltaFeedInline_Sets_NextPageLink_OnWriteEnd()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataDeltaResourceSet deltafeed = new ODataDeltaResourceSet { NextPageLink = new Uri("http://nextlink.com/") };
            Mock<ODataDeltaFeedSerializer> serializer = new Mock<ODataDeltaFeedSerializer>(_serializerProvider);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataDeltaFeed(instance, _customersType, _writeContext)).Returns(deltafeed);
            var mockWriter = new Mock<ODataWriter>();

            mockWriter.Setup(m => m.WriteStart(It.Is<ODataDeltaResourceSet>(f => f.NextPageLink == null))).Verifiable();
            mockWriter
                .Setup(m => m.WriteEnd())
                .Callback(() =>
                {
                    Assert.Equal("http://nextlink.com/", deltafeed.NextPageLink.AbsoluteUri);
                })
                .Verifiable();

            // Act
            serializer.Object.WriteDeltaFeedInline(instance, _customersType, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public void WriteDeltaFeedInline_Sets_DeltaLink()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataDeltaResourceSet deltafeed = new ODataDeltaResourceSet { DeltaLink = new Uri("http://deltalink.com/") };
            Mock<ODataDeltaFeedSerializer> serializer = new Mock<ODataDeltaFeedSerializer>(_serializerProvider);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataDeltaFeed(instance, _customersType, _writeContext)).Returns(deltafeed);
            var mockWriter = new Mock<ODataWriter>();

            mockWriter.Setup(m => m.WriteStart(deltafeed));
            mockWriter
                .Setup(m => m.WriteEnd())
                .Callback(() =>
                {
                    Assert.Equal("http://deltalink.com/", deltafeed.DeltaLink.AbsoluteUri);
                })
                .Verifiable();

            // Act
            serializer.Object.WriteDeltaFeedInline(instance, _customersType, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public void WriteDeltaFeedInline_Sets_DeltaResource_WithAnnotations()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataDeltaResourceSet deltafeed = new ODataDeltaResourceSet { DeltaLink = new Uri("http://deltalink.com/"), InstanceAnnotations = new List<ODataInstanceAnnotation>() { new ODataInstanceAnnotation("NS.Test", new ODataPrimitiveValue(1)) } };
            Mock<ODataDeltaFeedSerializer> serializer = new Mock<ODataDeltaFeedSerializer>(_serializerProvider);
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataDeltaFeed(instance, _customersType, _writeContext)).Returns(deltafeed);
            var mockWriter = new Mock<ODataWriter>();

            mockWriter.Setup(m => m.WriteStart(deltafeed));
            mockWriter
                .Setup(m => m.WriteEnd())
                .Callback(() =>
                {
                    Assert.Equal("http://deltalink.com/", deltafeed.DeltaLink.AbsoluteUri);
                })
                .Verifiable();

            // Act
            serializer.Object.WriteDeltaFeedInline(instance, _customersType, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public void CreateODataDeltaFeed_Sets_CountValueForPageResult()
        {
            // Arrange
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(_serializerProvider);
            Uri expectedNextLink = new Uri("http://nextlink.com");
            const long ExpectedCountValue = 1000;

            var result = new PageResult<Customer>(_customers, expectedNextLink, ExpectedCountValue);

            // Act
            ODataDeltaResourceSet feed = serializer.CreateODataDeltaFeed(result, _customersType, new ODataSerializerContext());

            // Assert
            Assert.Equal(ExpectedCountValue, feed.Count);
        }

        [Fact]
        public void CreateODataDeltaFeed_Sets_NextPageLinkForPageResult()
        {
            // Arrange
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(_serializerProvider);
            Uri expectedNextLink = new Uri("http://nextlink.com");
            const long ExpectedCountValue = 1000;

            var result = new PageResult<Customer>(_customers, expectedNextLink, ExpectedCountValue);

            // Act
            ODataDeltaResourceSet feed = serializer.CreateODataDeltaFeed(result, _customersType, new ODataSerializerContext());

            // Assert
            Assert.Equal(expectedNextLink, feed.NextPageLink);
        }
    }
}
