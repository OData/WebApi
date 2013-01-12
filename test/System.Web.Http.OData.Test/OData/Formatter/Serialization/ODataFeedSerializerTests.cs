// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
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
            SpyODataSerializer spy = new SpyODataSerializer(ODataPayloadKind.Entry);
            ODataSerializerProvider provider = new FakeODataSerializerProvider(spy);
            var mockWriter = new Mock<ODataWriter>();

            mockWriter
                .Setup(w => w.WriteStart(It.IsAny<ODataFeed>()))
                .Callback((ODataFeed feed) =>
                {
                    Assert.Equal("http://schemas.datacontract.org/2004/07/", feed.Id);
                })
                .Verifiable();

            _serializer = new ODataFeedSerializer(_customersType, provider);

            // Act
            _serializer.WriteObjectInline(_customers, mockWriter.Object, _writeContext);

            // Assert
            WriteObjectInlineCall[] expectedWriteCalls = new WriteObjectInlineCall[]
            {
                new WriteObjectInlineCall { Graph = _customers[0], WriteContext = _writeContext },
                new WriteObjectInlineCall { Graph = _customers[1], WriteContext = _writeContext }
            };
            AssertEqual(expectedWriteCalls, spy.WriteObjectInlineCalls);

            mockWriter.Verify();
        }

        [Fact]
        public void WriteObjectInline_Writes_InlineCountAndNextLink()
        {
            // Arrange
            SpyODataSerializer spy = new SpyODataSerializer(ODataPayloadKind.Entry);
            ODataSerializerProvider provider = new FakeODataSerializerProvider(spy);
            var mockWriter = new Mock<ODataWriter>();

            Uri expectedNextLink = new Uri("http://nextlink.com");
            long expectedInlineCount = 1000;

            var result = new PageResult<Customer>(
                _customers,
                expectedNextLink,
                expectedInlineCount
            );
            ODataFeed actualFeed = null;
            mockWriter
                .Setup(m => m.WriteStart(It.IsAny<ODataFeed>()))
                .Callback((ODataFeed feed) =>
                {
                    actualFeed = feed;
                    Assert.Equal(expectedInlineCount, feed.Count);
                });
            _serializer = new ODataFeedSerializer(_customersType, provider);

            _serializer.WriteObjectInline(result, mockWriter.Object, _writeContext);

            // Assert
            WriteObjectInlineCall[] expectedWriteCalls = new WriteObjectInlineCall[]
            {
                new WriteObjectInlineCall { Graph = _customers[0], WriteContext = _writeContext },
                new WriteObjectInlineCall { Graph = _customers[1], WriteContext = _writeContext }
            };
            AssertEqual(expectedWriteCalls, spy.WriteObjectInlineCalls);

            mockWriter.Verify();
            Assert.Equal(expectedNextLink, actualFeed.NextPageLink);
        }

        [Fact]
        public void WriteObjectInline_Writes_RequestNextPageLink()
        {
            // Arrange
            ODataSerializer customerSerializer = new StubODataSerializer(ODataPayloadKind.Entry);
            ODataSerializerProvider provider = new FakeODataSerializerProvider(customerSerializer);
            var mockWriter = new Mock<ODataWriter>();

            Uri expectedNextLink = new Uri("http://nextlink.com");
            _writeContext.NextPageLink = expectedNextLink;

            ODataFeed actualFeed = null;
            mockWriter
                .Setup(m => m.WriteStart(It.IsAny<ODataFeed>()))
                .Callback((ODataFeed feed) =>
                {
                    actualFeed = feed;
                });
            _serializer = new ODataFeedSerializer(_customersType, provider);

            // Act
            _serializer.WriteObjectInline(_customers, mockWriter.Object, _writeContext);

            // Assert
            Assert.Equal(expectedNextLink, actualFeed.NextPageLink);
        }

        [Fact]
        public void WriteObjectInline_Writes_RequestCount()
        {
            // Arrange
            ODataSerializer customerSerializer = new StubODataSerializer(ODataPayloadKind.Entry);
            ODataSerializerProvider provider = new FakeODataSerializerProvider(customerSerializer);
            var mockWriter = new Mock<ODataWriter>();

            long expectedCount = 12345;
            _writeContext.InlineCount = expectedCount;

            mockWriter
                .Setup(m => m.WriteStart(It.IsAny<ODataFeed>()))
                .Callback((ODataFeed feed) =>
                {
                    Assert.Equal(expectedCount, feed.Count);
                });
            _serializer = new ODataFeedSerializer(_customersType, provider);

            // Act
            _serializer.WriteObjectInline(_customers, mockWriter.Object, _writeContext);

            // Assert
            mockWriter.Verify();
        }

        private static void AssertEqual(IList<WriteObjectInlineCall> expected, IList<WriteObjectInlineCall> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            for (int index = 0; index < expected.Count; index++)
            {
                Assert.Equal(expected[index], actual[index]);
            }
        }

        private class SpyODataSerializer : ODataSerializer
        {
            public SpyODataSerializer(ODataPayloadKind payloadKind)
                : base(payloadKind)
            {
                WriteObjectInlineCalls = new List<WriteObjectInlineCall>();
            }

            public IList<WriteObjectInlineCall> WriteObjectInlineCalls { get; private set; }

            public override void WriteObjectInline(object graph, ODataWriter writer, ODataSerializerContext writeContext)
            {
                WriteObjectInlineCall call = new WriteObjectInlineCall
                {
                    Graph = graph,
                    WriteContext = writeContext,
                };

                WriteObjectInlineCalls.Add(call);
            }
        }

        private class StubODataSerializer : ODataSerializer
        {
            public StubODataSerializer(ODataPayloadKind payloadKind)
                : base(payloadKind)
            {
            }

            public override void WriteObjectInline(object graph, ODataWriter writer,
                ODataSerializerContext writeContext)
            {
            }
        }

        private class WriteObjectInlineCall
        {
            public object Graph { get; set; }

            public ODataSerializerContext WriteContext { get; set; }

            public override bool Equals(object obj)
            {
                WriteObjectInlineCall other = obj as WriteObjectInlineCall;

                if (other == null)
                {
                    return false;
                }

                bool equal = object.ReferenceEquals(Graph, other.Graph) &&
                    object.ReferenceEquals(WriteContext, other.WriteContext);
                return equal;
            }

            public override int GetHashCode()
            {
                return Graph.GetHashCode() ^ WriteContext.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format(CultureInfo.CurrentCulture, "Graph: {0}, WriteContext: {1}", Graph, WriteContext);
            }
        }
    }
}
