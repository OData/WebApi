﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.OData.Formatter.Serialization.Models;
using System.Web.OData.Routing;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace System.Web.OData.Formatter.Serialization
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
        public ODataDeltaFeedSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _customerSet = _model.EntityContainer.FindEntitySet("Customers");
            _model.SetAnnotationValue(_customerSet.EntityType(), new ClrTypeAnnotation(typeof(Customer)));
            _path = new ODataPath(new EntitySetPathSegment(_customerSet));
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

            _deltaFeedCustomers = new EdmChangedObjectCollection(_customerSet.EntityType());
            EdmDeltaEntityObject newCustomer = new EdmDeltaEntityObject(_customerSet.EntityType());
            newCustomer.TrySetPropertyValue("ID", 10);
            newCustomer.TrySetPropertyValue("FirstName", "Foo");
            _deltaFeedCustomers.Add(newCustomer);

             _customersType = _model.GetEdmTypeReference(typeof(Customer[])).AsCollection();

            _writeContext = new ODataSerializerContext() { NavigationSource = _customerSet, Model = _model, Path = _path };
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_SerializerProvider()
        {
            Assert.ThrowsArgumentNull(
                () => new ODataDeltaFeedSerializer(serializerProvider: null),
                "serializerProvider");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_MessageWriter()
        {
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(new DefaultODataSerializerProvider());
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: null, messageWriter: null, writeContext: new ODataSerializerContext()),
                "messageWriter");
        }

        [Fact]
        public void WriteObject_ThrowsArgumentNull_WriteContext()
        {
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(new DefaultODataSerializerProvider());
            Assert.ThrowsArgumentNull(
                () => serializer.WriteObject(graph: null, type: null, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteObject_ThrowsEntitySetMissingDuringSerialization()
        {
            object graph = new object();
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(new DefaultODataSerializerProvider());
            Assert.Throws<SerializationException>(
                () => serializer.WriteObject(graph: graph, type: null, messageWriter: ODataTestUtil.GetMockODataMessageWriter(), writeContext: new ODataSerializerContext()),
                "The related entity set could not be found from the OData path. The related entity set is required to serialize the payload.");
        }

        [Fact]
        public void WriteObject_Calls_WriteDeltaFeedInline()
        {
            // Arrange
            object graph = new object();
            Mock<ODataDeltaFeedSerializer> serializer = new Mock<ODataDeltaFeedSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer
                .Setup(s => s.WriteDeltaFeedInline(graph, It.Is<IEdmTypeReference>(e => _customersType.IsEquivalentTo(e)),
                    It.IsAny<ODataDeltaWriter>(), _writeContext))
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
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(new DefaultODataSerializerProvider());
            Assert.ThrowsArgumentNull(
                () => serializer.WriteDeltaFeedInline(graph: null, expectedType: null, writer: null, writeContext: new ODataSerializerContext()),
                "writer");
        }

        [Fact]
        public void WriteDeltaFeedInline_ThrowsArgumentNull_WriteContext()
        {
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(new DefaultODataSerializerProvider());
            Assert.ThrowsArgumentNull(
                () => serializer.WriteDeltaFeedInline(graph: null, expectedType: null, writer: new Mock<ODataDeltaWriter>().Object, writeContext: null),
                "writeContext");
        }

        [Fact]
        public void WriteDeltaFeedInline_ThrowsSerializationException_CannotSerializerNull()
        {
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(new DefaultODataSerializerProvider());
            Assert.Throws<SerializationException>(
                () => serializer.WriteDeltaFeedInline(graph: null, expectedType: _customersType,
                    writer: new Mock<ODataDeltaWriter>().Object, writeContext: _writeContext),
                "Cannot serialize a null 'deltafeed'.");
        }

        [Fact]
        public void WriteDeltaFeedInline_ThrowsSerializationException_IfGraphIsNotEnumerable()
        {
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(new DefaultODataSerializerProvider());
            Assert.Throws<SerializationException>(
                () => serializer.WriteDeltaFeedInline(graph: 42, expectedType: _customersType,
                    writer: new Mock<ODataDeltaWriter>().Object, writeContext: _writeContext),
                "ODataDeltaFeedSerializer cannot write an object of type 'System.Int32'.");
        }

        [Fact]
        public void WriteDeltaFeedInline_Throws_NullElementInCollection_IfFeedContainsNullElement()
        {
            // Arrange
            IEnumerable instance = new object[] { null };
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(new DefaultODataSerializerProvider());

            // Act
            Assert.Throws<SerializationException>(
                () => serializer.WriteDeltaFeedInline(instance, _customersType, new Mock<ODataDeltaWriter>().Object, _writeContext),
                "Collections cannot contain null elements.");
        }

        [Fact]
        public void WriteDeltaFeedInline_Throws_TypeCannotBeSerialized_IfFeedContainsEntityThatCannotBeSerialized()
        {
            // Arrange
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            HttpRequestMessage request = new HttpRequestMessage();
            serializerProvider.Setup(s => s.GetODataPayloadSerializer(_model, typeof(int), request)).Returns<ODataSerializer>(null);
            IEnumerable instance = new object[] { 42 };
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(serializerProvider.Object);

            // Act
            Assert.Throws<SerializationException>(
                () => serializer.WriteDeltaFeedInline(instance, _customersType, new Mock<ODataDeltaWriter>().Object, _writeContext),
                "ODataDeltaFeedSerializer cannot write an object of type 'System.Object[]'.");
        }

        [Fact]
        public void WriteDeltaFeedInline_Calls_CreateODataDeltaFeed()
        {
            // Arrange
            IEnumerable instance = new object[0];
            Mock<ODataDeltaFeedSerializer> serializer = new Mock<ODataDeltaFeedSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataDeltaFeed(instance, _customersType, _writeContext)).Returns(new ODataDeltaFeed()).Verifiable();

            // Act
            serializer.Object.WriteDeltaFeedInline(instance, _customersType, new Mock<ODataDeltaWriter>().Object, _writeContext);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteDeltaFeedInline_Throws_CannotSerializerNull_IfCreateODataDeltaFeedReturnsNull()
        {
            // Arrange
            IEnumerable instance = new object[0];
            Mock<ODataDeltaFeedSerializer> serializer = new Mock<ODataDeltaFeedSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataDeltaFeed(instance, _customersType, _writeContext)).Returns<ODataDeltaFeed>(null);
            ODataDeltaWriter writer = new Mock<ODataDeltaWriter>().Object;

            // Act & Assert
            Assert.Throws<SerializationException>(
                () => serializer.Object.WriteDeltaFeedInline(instance, _customersType, writer, _writeContext),
                "Cannot serialize a null 'deltafeed'.");
        }

        [Fact]
        public void WriteDeltaFeedInline_Writes_CreateODataFeedOutput()
        {
            // Arrange
            IEnumerable instance = new object[0];
            ODataDeltaFeed deltafeed = new ODataDeltaFeed();
            Mock<ODataDeltaFeedSerializer> serializer = new Mock<ODataDeltaFeedSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataDeltaFeed(instance, _customersType, _writeContext)).Returns(deltafeed);
            Mock<ODataDeltaWriter> writer = new Mock<ODataDeltaWriter>();
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

            var mockWriter = new Mock<ODataDeltaWriter>();

            Mock<ODataEntityTypeSerializer> customerSerializer = new Mock<ODataEntityTypeSerializer>(new DefaultODataSerializerProvider());
            customerSerializer.Setup(s => s.WriteDeltaObjectInline(edmObject.Object, edmType, mockWriter.Object, _writeContext)).Verifiable();

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(edmType)).Returns(customerSerializer.Object);

            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(serializerProvider.Object);

            // Act
            serializer.WriteDeltaFeedInline(new[] { edmObject.Object }, feedType, mockWriter.Object, _writeContext);

            // Assert
            customerSerializer.Verify();
        }

        [Fact]
        public void WriteDeltaFeedInline_WritesEachEntityInstance()
        {
            // Arrange
            Mock<ODataEntityTypeSerializer> customerSerializer = new Mock<ODataEntityTypeSerializer>(new DefaultODataSerializerProvider());
            ODataSerializerProvider provider = ODataTestUtil.GetMockODataSerializerProvider(customerSerializer.Object);
            var mockWriter = new Mock<ODataDeltaWriter>();
            customerSerializer.Setup(s => s.WriteDeltaObjectInline(_deltaFeedCustomers[0], _customersType.ElementType(), mockWriter.Object, _writeContext)).Verifiable();
            _serializer = new ODataDeltaFeedSerializer(provider);

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
            ODataDeltaFeed deltafeed = new ODataDeltaFeed { NextPageLink = new Uri("http://nextlink.com/") };
            Mock<ODataDeltaFeedSerializer> serializer = new Mock<ODataDeltaFeedSerializer>(new DefaultODataSerializerProvider());
            serializer.CallBase = true;
            serializer.Setup(s => s.CreateODataDeltaFeed(instance, _customersType, _writeContext)).Returns(deltafeed);
            var mockWriter = new Mock<ODataDeltaWriter>();

            mockWriter.Setup(m => m.WriteStart(It.Is<ODataDeltaFeed>(f => f.NextPageLink == null))).Verifiable();
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
        public void CreateODataDeltaFeed_Sets_CountValueForPageResult()
        {
            // Arrange
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(new DefaultODataSerializerProvider());
            Uri expectedNextLink = new Uri("http://nextlink.com");
            const long ExpectedCountValue = 1000;

            var result = new PageResult<Customer>(_customers, expectedNextLink, ExpectedCountValue);

            // Act
            ODataDeltaFeed feed = serializer.CreateODataDeltaFeed(result, _customersType, new ODataSerializerContext());

            // Assert
            Assert.Equal(ExpectedCountValue, feed.Count);
        }

        [Fact]
        public void CreateODataDeltaFeed_Sets_NextPageLinkForPageResult()
        {
            // Arrange
            ODataDeltaFeedSerializer serializer = new ODataDeltaFeedSerializer(new DefaultODataSerializerProvider());
            Uri expectedNextLink = new Uri("http://nextlink.com");
            const long ExpectedCountValue = 1000;

            var result = new PageResult<Customer>(_customers, expectedNextLink, ExpectedCountValue);

            // Act
            ODataDeltaFeed feed = serializer.CreateODataDeltaFeed(result, _customersType, new ODataSerializerContext());

            // Assert
            Assert.Equal(expectedNextLink, feed.NextPageLink);
        }
    }
}