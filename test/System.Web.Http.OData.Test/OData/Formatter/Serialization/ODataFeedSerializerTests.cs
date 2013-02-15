// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Formatter.Serialization.Models;
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

            _writeContext = new ODataSerializerContext() { EntitySet = _customerSet, Model = _model };
        }

        [Fact]
        public void WriteObjectInline_WritesEachEntityInstance()
        {
            // Arrange
            Mock<ODataEntrySerializer> customerSerializer = new Mock<ODataEntrySerializer>(_customersType.ElementType(), ODataPayloadKind.Entry);
            ODataSerializerProvider provider = ODataTestUtil.GetMockODataSerializerProvider(customerSerializer.Object);
            var mockWriter = new Mock<ODataWriter>();

            customerSerializer.Setup(s => s.WriteObjectInline(_customers[0], mockWriter.Object, _writeContext)).Verifiable();
            customerSerializer.Setup(s => s.WriteObjectInline(_customers[1], mockWriter.Object, _writeContext)).Verifiable();

            _serializer = new ODataFeedSerializer(_customersType, provider);

            // Act
            _serializer.WriteObjectInline(_customers, mockWriter.Object, _writeContext);

            // Assert
            customerSerializer.Verify();
        }

        [Fact]
        public void WriteObjectInline_Sets_InlineCount_OnWriteStart()
        {
            // Arrange
            Mock<ODataEntrySerializer> customerSerializer = new Mock<ODataEntrySerializer>(_customersType.ElementType(), ODataPayloadKind.Entry);
            ODataSerializerProvider provider = ODataTestUtil.GetMockODataSerializerProvider(customerSerializer.Object);
            var mockWriter = new Mock<ODataWriter>();

            Uri expectedNextLink = new Uri("http://nextlink.com");
            long expectedInlineCount = 1000;

            var result = new PageResult<Customer>(_customers, expectedNextLink, expectedInlineCount);
            mockWriter
                .Setup(m => m.WriteStart(It.Is<ODataFeed>(feed => feed.Count == expectedInlineCount)))
                .Verifiable();

            _serializer = new ODataFeedSerializer(_customersType, provider);

            // Act
            _serializer.WriteObjectInline(result, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public void WriteObjectInline_Sets_NextPageLink_OnWriteEnd()
        {
            // Arrange
            Mock<ODataEntrySerializer> customerSerializer = new Mock<ODataEntrySerializer>(_customersType.ElementType(), ODataPayloadKind.Entry);
            ODataSerializerProvider provider = ODataTestUtil.GetMockODataSerializerProvider(customerSerializer.Object);
            var mockWriter = new Mock<ODataWriter>();

            Uri expectedNextLink = new Uri("http://nextlink.com");

            var result = new PageResult<Customer>(_customers, expectedNextLink, count: 0);
            ODataFeed actualFeed = null;
            mockWriter
                .Setup(m => m.WriteStart(It.Is<ODataFeed>(feed => feed.NextPageLink == null)))
                .Callback<ODataFeed>(feed => { actualFeed = feed; })
                .Verifiable();
            mockWriter
                .Setup(m => m.WriteEnd())
                .Callback(() =>
                {
                    if (actualFeed != null)
                    {
                        Assert.Equal(expectedNextLink, actualFeed.NextPageLink);
                    }
                })
                .Verifiable();

            _serializer = new ODataFeedSerializer(_customersType, provider);

            // Act
            _serializer.WriteObjectInline(result, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public void WriteObjectInline_Sets_Request_NextPageLink_OnWriteEnd()
        {
            // Arrange
            Mock<ODataEntrySerializer> customerSerializer = new Mock<ODataEntrySerializer>(_customersType.ElementType(), ODataPayloadKind.Entry);
            ODataSerializerProvider provider = ODataTestUtil.GetMockODataSerializerProvider(customerSerializer.Object);
            var mockWriter = new Mock<ODataWriter>();

            Uri expectedNextLink = new Uri("http://nextlink.com");
            _writeContext.NextPageLink = expectedNextLink;

            ODataFeed actualFeed = null;
            mockWriter
                 .Setup(m => m.WriteStart(It.Is<ODataFeed>(feed => feed.NextPageLink == null)))
                 .Callback<ODataFeed>(feed => { actualFeed = feed; })
                 .Verifiable();
            mockWriter
                .Setup(m => m.WriteEnd())
                .Callback(() =>
                {
                    if (actualFeed != null)
                    {
                        Assert.Equal(expectedNextLink, actualFeed.NextPageLink);
                    }
                })
                .Verifiable();
            _serializer = new ODataFeedSerializer(_customersType, provider);

            // Act
            _serializer.WriteObjectInline(_customers, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        [Fact]
        public void WriteObjectInline_Sets_Request_InlineCount_OnWriteStart()
        {
            // Arrange
            Mock<ODataEntrySerializer> customerSerializer = new Mock<ODataEntrySerializer>(_customersType.ElementType(), ODataPayloadKind.Entry);
            ODataSerializerProvider provider = ODataTestUtil.GetMockODataSerializerProvider(customerSerializer.Object);
            var mockWriter = new Mock<ODataWriter>();

            long expectedCount = 12345;
            _writeContext.InlineCount = expectedCount;

            mockWriter
                .Setup(m => m.WriteStart(It.Is<ODataFeed>(feed => feed.Count == 12345)))
                .Verifiable();
            _serializer = new ODataFeedSerializer(_customersType, provider);

            // Act
            _serializer.WriteObjectInline(_customers, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }
    }
}
