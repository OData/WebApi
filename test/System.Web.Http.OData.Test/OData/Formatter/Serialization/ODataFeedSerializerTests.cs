// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.OData.Formatter.Serialization.Models;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter.Serialization
{
    public class ODataFeedSerializerTests
    {
        IEdmModel _model;
        IEdmEntitySet _customerSet;
        Customer[] _customers;
        ODataFeedSerializer _serializer;
        IEdmCollectionTypeReference _customersType;
        UrlHelper _urlHelper;
        ODataSerializerContext _writeContext;

        public ODataFeedSerializerTests()
        {
            _model = SerializationTestsHelpers.SimpleCustomerOrderModel();
            _customerSet = _model.FindDeclaredEntityContainer("Default.Container").FindEntitySet("Customers");
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

            _customersType = new EdmCollectionTypeReference(
                    new EdmCollectionType(
                        new EdmEntityTypeReference(
                            _customerSet.ElementType,
                            isNullable: false)),
                    isNullable: false);

            _urlHelper = new Mock<UrlHelper>(new HttpRequestMessage()).Object;
            _writeContext = new ODataSerializerContext() { EntitySet = _customerSet, UrlHelper = _urlHelper };
        }

        [Fact]
        public void WriteObjectInline_WritesEachEntityInstance()
        {
            // Arrange
            var mockSerializerProvider = new Mock<ODataSerializerProvider>(MockBehavior.Strict, _model);
            var mockCustomerSerializer = new Mock<ODataSerializer>(MockBehavior.Strict, ODataPayloadKind.Entry);
            var mockWriter = new Mock<ODataWriter>();

            mockSerializerProvider
                .Setup(p => p.GetODataPayloadSerializer(typeof(Customer)))
                .Returns(mockCustomerSerializer.Object);
            mockCustomerSerializer
                .Setup(s => s.WriteObjectInline(_customers[0], It.IsAny<ODataWriter>(), _writeContext))
                .Verifiable();
            mockCustomerSerializer
                .Setup(s => s.WriteObjectInline(_customers[1], It.IsAny<ODataWriter>(), _writeContext))
                .Verifiable();
            mockWriter
                .Setup(w => w.WriteStart(It.IsAny<ODataFeed>()))
                .Callback((ODataFeed feed) =>
                {
                    Assert.Equal("http://schemas.datacontract.org/2004/07/", feed.Id);
                })
                .Verifiable();

            _serializer = new ODataFeedSerializer(_customersType, mockSerializerProvider.Object);

            // Act
            _serializer.WriteObjectInline(_customers, mockWriter.Object, _writeContext);

            // Assert
            mockSerializerProvider.Verify();
            mockCustomerSerializer.Verify();
            mockWriter.Verify();
        }

        [Fact]
        public void WriteObjectInline_Writes_InlineCountAndNextLink()
        {
            // Arrange
            var mockSerializerProvider = new Mock<ODataSerializerProvider>(MockBehavior.Strict, _model);
            var mockCustomerSerializer = new Mock<ODataSerializer>(MockBehavior.Strict, ODataPayloadKind.Entry);
            var mockWriter = new Mock<ODataWriter>();

            Uri expectedNextLink = new Uri("http://nextlink.com");
            long expectedInlineCount = 1000;

            var result = new ODataResult<Customer>(
                _customers,
                expectedNextLink,
                expectedInlineCount
            );
            mockSerializerProvider
                .Setup(p => p.GetODataPayloadSerializer(typeof(Customer)))
                .Returns(mockCustomerSerializer.Object);
            mockCustomerSerializer
                .Setup(s => s.WriteObjectInline(_customers[0], It.IsAny<ODataWriter>(), _writeContext))
                .Verifiable();
            mockCustomerSerializer
                .Setup(s => s.WriteObjectInline(_customers[1], It.IsAny<ODataWriter>(), _writeContext))
                .Verifiable();
            ODataFeed actualFeed = null;
            mockWriter
                .Setup(m => m.WriteStart(It.IsAny<ODataFeed>()))
                .Callback((ODataFeed feed) =>
                {
                    actualFeed = feed;
                    Assert.Equal(expectedInlineCount, feed.Count);
                });
            _serializer = new ODataFeedSerializer(_customersType, mockSerializerProvider.Object);

            _serializer.WriteObjectInline(result, mockWriter.Object, _writeContext);

            // Assert
            mockSerializerProvider.Verify();
            mockCustomerSerializer.Verify();
            mockWriter.Verify();
            Assert.Equal(expectedNextLink, actualFeed.NextPageLink);
        }

        [Fact]
        public void WriteObjectInline_Writes_RequestNextPageLink()
        {
            // Arrange
            var mockSerializerProvider = new Mock<ODataSerializerProvider>(_model);
            var mockCustomerSerializer = new Mock<ODataSerializer>(ODataPayloadKind.Entry);
            var mockWriter = new Mock<ODataWriter>();

            Uri expectedNextLink = new Uri("http://nextlink.com");
            _writeContext.NextPageLink = expectedNextLink;

            mockSerializerProvider
                .Setup(p => p.GetODataPayloadSerializer(typeof(Customer)))
                .Returns(mockCustomerSerializer.Object);
            ODataFeed actualFeed = null;
            mockWriter
                .Setup(m => m.WriteStart(It.IsAny<ODataFeed>()))
                .Callback((ODataFeed feed) =>
                {
                    actualFeed = feed;
                });
            _serializer = new ODataFeedSerializer(_customersType, mockSerializerProvider.Object);

            _serializer.WriteObjectInline(_customers, mockWriter.Object, _writeContext);

            Assert.Equal(expectedNextLink, actualFeed.NextPageLink);
        }

        [Fact]
        public void WriteObjectInline_Writes_RequestCount()
        {
            // Arrange
            var mockSerializerProvider = new Mock<ODataSerializerProvider>(_model);
            var mockCustomerSerializer = new Mock<ODataSerializer>(ODataPayloadKind.Entry);
            var mockWriter = new Mock<ODataWriter>();

            long expectedCount = 12345;
            _writeContext.InlineCount = expectedCount;

            mockSerializerProvider
                .Setup(p => p.GetODataPayloadSerializer(typeof(Customer)))
                .Returns(mockCustomerSerializer.Object);
            mockWriter
                .Setup(m => m.WriteStart(It.IsAny<ODataFeed>()))
                .Callback((ODataFeed feed) =>
                {
                    Assert.Equal(expectedCount, feed.Count);
                });
            _serializer = new ODataFeedSerializer(_customersType, mockSerializerProvider.Object);

            _serializer.WriteObjectInline(_customers, mockWriter.Object, _writeContext);
        }
    }
}
